using System;
using System.Threading;
using System.Threading.Tasks;

#if ANDROID
using AApp = Android.App.Application;
using AActivity = Android.App.Activity;
using ABundle = Android.OS.Bundle;
using AHandler = Android.OS.Handler;
using ALooper = Android.OS.Looper;
using AContext = Android.Content.Context;
using CManager = Android.Net.ConnectivityManager;
using NRequest = Android.Net.NetworkRequest;
using NCapability = Android.Net.NetCapability;
using ANetwork = Android.Net.Network;
#elif IOS
using Foundation;
using UIKit;
using System.Net;
using SystemConfiguration;
using CoreFoundation;
#endif

using AppAmbit.Services;

namespace AppAmbit;

internal static class MauiNativePlatforms
{
    private static string? _appKey;
    private static bool _readyForBatches = false;
    private static DateTime _lastSendAllAtUtc = DateTime.MinValue;
    private static readonly TimeSpan _minSendInterval = TimeSpan.FromSeconds(1);
    private static readonly SemaphoreSlim _connectivityGate = new(1, 1);

#if ANDROID
    internal static bool _initialized = false;
    private static int _startedActivities = 0;
    private static int _resumedActivities = 0;
    private static bool _foreground = false;
    private static bool _isWaitingPause = false;
    private static readonly long _activityDelay = 700;
    private static readonly AHandler _handler = new AHandler(ALooper.MainLooper);
    private static CManager.NetworkCallback? _netCallback;
    private static readonly Java.Lang.IRunnable _pauseRunnable = new PauseRunnable();
    private static bool _androidPageBreadcrumbsEnabled = false;

    private sealed class PauseRunnable : Java.Lang.Object, Java.Lang.IRunnable
    {
        public void Run()
        {
            if (_resumedActivities == 0 && _foreground && _isWaitingPause)
            {
                _foreground = false;
                AppAmbitSdk.InternalSleep();
            }
            _isWaitingPause = false;
        }
    }
#endif

#if IOS
    private static NSObject? _obsDidBecomeActive;
    private static NSObject? _obsWillResignActive;
    private static NSObject? _obsDidEnterBackground;
    private static NSObject? _obsWillEnterForeground;
    private static NSObject? _obsWillTerminate;

    private static NetworkReachability? _reachability;
    private static bool? _iosLastOnline = null;

    private static bool _iosPageBreadcrumbsEnabled = false;
    private static bool _iosPagesHooked = false;
    private static UINavigationControllerDelegate? _navDelegate;

    private sealed class NavDelegate : UINavigationControllerDelegate
    {
        public override void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            _ = BreadcrumbManager.AddAsync("page_disappear");
        }
        public override void DidShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            _ = BreadcrumbManager.AddAsync("page_appear");
        }
    }
#endif

    public static void Register(string appKey)
    {
        _appKey = appKey;

#if ANDROID
        var app = AApp.Context as AApp;
        if (app == null) return;
        if (_initialized) return;

        app.RegisterActivityLifecycleCallbacks(new LifecycleCallbacks());
        RegisterNetworkCallback();
        _initialized = true;
        _ = OnStartAppAsync();

#elif IOS
        if (_obsDidBecomeActive != null) return;

        var nc = NSNotificationCenter.DefaultCenter;
        _obsDidBecomeActive     = nc.AddObserver(UIApplication.DidBecomeActiveNotification, HandleDidBecomeActive);
        _obsWillResignActive    = nc.AddObserver(UIApplication.WillResignActiveNotification, HandleWillResignActive);
        _obsDidEnterBackground  = nc.AddObserver(UIApplication.DidEnterBackgroundNotification, HandleDidEnterBackground);
        _obsWillEnterForeground = nc.AddObserver(UIApplication.WillEnterForegroundNotification, HandleWillEnterForeground);
        _obsWillTerminate       = nc.AddObserver(UIApplication.WillTerminateNotification, HandleWillTerminate);

        StartReachability();
        _ = OnStartAppAsync();
#endif
    }

    public static void EnableNativePageBreadcrumbs()
    {
#if ANDROID
        _androidPageBreadcrumbsEnabled = true;
#endif
#if IOS
        _iosPageBreadcrumbsEnabled = true;
        EnsureIOSPagesHookedOnMain();
#endif
    }

#if ANDROID
    private sealed class LifecycleCallbacks : Java.Lang.Object, AApp.IActivityLifecycleCallbacks
    {
        public void OnActivityCreated(AActivity activity, ABundle? savedInstanceState) { }

        public void OnActivityStarted(AActivity activity)
        {
            _startedActivities++;
        }

        public void OnActivityResumed(AActivity activity)
        {
            _resumedActivities++;
            if (_isWaitingPause)
            {
                _handler.RemoveCallbacks(_pauseRunnable);
                _isWaitingPause = false;
            }
            if (!_foreground)
            {
                _foreground = true;
                _ = OnResumeAppAsync();
            }

            if (_androidPageBreadcrumbsEnabled)
            {
                _ = BreadcrumbManager.AddAsync("page_appear");
            }
        }

        public void OnActivityPaused(AActivity activity)
        {
            _resumedActivities = Math.Max(0, _resumedActivities - 1);
            if (_resumedActivities == 0)
            {
                _isWaitingPause = true;
                _handler.PostDelayed(_pauseRunnable, _activityDelay);
            }

            if (_androidPageBreadcrumbsEnabled)
            {
                _ = BreadcrumbManager.AddAsync("page_disappear");
            }
        }

        public void OnActivityStopped(AActivity activity)
        {
            _startedActivities = Math.Max(0, _startedActivities - 1);
            if (_startedActivities == 0 && !activity.IsChangingConfigurations)
            {
                AppAmbitSdk.InternalSleep();
            }
        }

        public void OnActivitySaveInstanceState(AActivity activity, ABundle outState) { }

        public void OnActivityDestroyed(AActivity activity)
        {
            if (_startedActivities == 0 && _resumedActivities == 0 && !activity.IsChangingConfigurations)
            {
                AppAmbitSdk.InternalSleep();
            }
        }
    }

    private static void RegisterNetworkCallback()
    {
        var cm = (CManager?)AApp.Context.GetSystemService(AContext.ConnectivityService);
        if (cm == null) return;

        if (OperatingSystem.IsAndroidVersionAtLeast(23))
        {
            var granted = AApp.Context.CheckSelfPermission(global::Android.Manifest.Permission.AccessNetworkState) == Android.Content.PM.Permission.Granted;
            if (!granted) return;
        }

        try
        {
            var request = new NRequest.Builder()
                .AddCapability(NCapability.Internet)
                .Build();

            _netCallback ??= new NetCb();
            cm.RegisterNetworkCallback(request, _netCallback);
        }
        catch (Java.Lang.SecurityException) { _netCallback = null; }
        catch { }
    }

    private sealed class NetCb : CManager.NetworkCallback
    {
        public override void OnAvailable(ANetwork network)
        {
            base.OnAvailable(network);
            _ = BreadcrumbManager.AddAsync("online");
            _handler.PostDelayed(new Java.Lang.Runnable(() =>
            {
                _ = OnConnectivityAvailableAsync();
            }), 3000);
        }
        public override void OnLost(ANetwork network)
        {
            base.OnLost(network);
            _ = BreadcrumbManager.AddAsync("offline");
        }
    }
#endif

#if IOS
    private static void StartReachability()
    {
        try
        {
            if (_reachability != null) return;

            _reachability = new NetworkReachability(new IPAddress(0));
            _reachability.SetNotification(OnReachabilityChanged);
            _reachability.Schedule(CFRunLoop.Main, CFRunLoop.ModeDefault);

            if (_reachability.TryGetFlags(out var flags))
            {
                var online = IsReachable(flags);
                _iosLastOnline = online;
                _ = BreadcrumbManager.AddAsync(online ? "online" : "offline");
                if (online) _ = OnConnectivityAvailableAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] StartReachability: {ex}");
        }
    }

    private static void OnReachabilityChanged(NetworkReachabilityFlags flags)
    {
        try
        {
            var online = IsReachable(flags);
            if (_iosLastOnline != online)
            {
                _iosLastOnline = online;
                _ = BreadcrumbManager.AddAsync(online ? "online" : "offline");
                if (online) _ = OnConnectivityAvailableAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] OnReachabilityChanged: {ex}");
        }
    }

    private static bool IsReachable(NetworkReachabilityFlags flags)
    {
        var reachable = (flags & NetworkReachabilityFlags.Reachable) != 0;
        var noConnectionRequired = (flags & NetworkReachabilityFlags.ConnectionRequired) == 0;
        var canConnectAutomatically =
            (flags & NetworkReachabilityFlags.ConnectionOnDemand) != 0 ||
            (flags & NetworkReachabilityFlags.ConnectionOnTraffic) != 0;
        var canConnectWithoutUser =
            canConnectAutomatically && (flags & NetworkReachabilityFlags.InterventionRequired) == 0;

        return reachable && (noConnectionRequired || canConnectWithoutUser);
    }

    private static void HandleDidBecomeActive(NSNotification n)
    {
        try
        {
            _ = AppAmbitSdk.InternalResume();
            EnsureIOSPagesHookedOnMain();
            _ = OnResumeAppAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] DidBecomeActive: {ex}");
        }
    }

    private static void HandleWillResignActive(NSNotification n)
    {
        try { AppAmbitSdk.InternalSleep(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] WillResignActive: {ex}"); }
    }

    private static void HandleDidEnterBackground(NSNotification n)
    {
        try { AppAmbitSdk.InternalSleep(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] DidEnterBackground: {ex}"); }
    }

    private static void HandleWillEnterForeground(NSNotification n)
    {
        try
        {
            _ = AppAmbitSdk.InternalResume();
            EnsureIOSPagesHookedOnMain();
            _ = OnResumeAppAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] WillEnterForeground: {ex}");
        }
    }

    private static void HandleWillTerminate(NSNotification n)
    {
        try { AppAmbitSdk.InternalEnd(); } catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] WillTerminate: {ex}"); }
    }

    private static void EnsureIOSPagesHookedOnMain()
    {
        if (!_iosPageBreadcrumbsEnabled || _iosPagesHooked) return;
        UIApplication.SharedApplication.InvokeOnMainThread(TryHookIOSNativePages);
    }

    private static void TryHookIOSNativePages()
    {
        if (_iosPagesHooked) return;

        var scenes = UIApplication.SharedApplication.ConnectedScenes;
        if (scenes != null && scenes.Count > 0)
        {
            foreach (var sceneObj in scenes)
            {
                if (sceneObj is UIWindowScene ws && ws.Windows != null)
                {
                    foreach (var window in ws.Windows)
                    {
                        var nav = FindNavController(window.RootViewController);
                        if (nav != null)
                        {
                            _navDelegate ??= new NavDelegate();
                            nav.Delegate = _navDelegate;
                            _iosPagesHooked = true;
                            return;
                        }
                    }
                }
            }
        }

        var legacyWindows = UIApplication.SharedApplication.Windows;
        if (legacyWindows != null)
        {
            foreach (var window in legacyWindows)
            {
                var nav = FindNavController(window.RootViewController);
                if (nav != null)
                {
                    _navDelegate ??= new NavDelegate();
                    nav.Delegate = _navDelegate;
                    _iosPagesHooked = true;
                    return;
                }
            }
        }
    }

    private static UINavigationController? FindNavController(UIViewController? vc)
    {
        if (vc == null) return null;
        if (vc is UINavigationController nvc) return nvc;

        if (vc.PresentedViewController != null)
        {
            var p = FindNavController(vc.PresentedViewController);
            if (p != null) return p;
        }

        if (vc is UITabBarController tab && tab.SelectedViewController != null)
        {
            var t = FindNavController(tab.SelectedViewController);
            if (t != null) return t;
        }

        var children = vc.ChildViewControllers;
        if (children != null)
        {
            foreach (var child in children)
            {
                var c = FindNavController(child);
                if (c != null) return c;
            }
        }
        return null;
    }
#endif

    private static async Task OnStartAppAsync()
    {
        try
        {
            await AppAmbitSdk.InternalStart(_appKey ?? string.Empty);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] OnStartAppAsync: {ex}");
        }
    }

    private static async Task OnResumeAppAsync()
    {
        if (Analytics._isManualSessionEnabled || !_readyForBatches)
        {
            _readyForBatches = true;
            return;
        }

        try
        {
#if ANDROID
            if (!AppAmbitSdk.InternalTokenIsValid())
                await AppAmbitSdk.InternalEnsureToken(null);

            if (!Analytics._isManualSessionEnabled)
                await SessionManager.RemoveSavedEndSession();

            await Analytics.SendBatchEvents();
            await Crashes.SendBatchLogs();
            await BreadcrumbManager.SendPending();
#else
            if (!AppAmbitSdk.InternalTokenIsValid())
                await AppAmbitSdk.InternalEnsureToken(null);

            if (!Analytics._isManualSessionEnabled)
                await SessionManager.RemoveSavedEndSession();

            await Crashes.LoadCrashFileIfExists();
            await SendAllPendingThrottledAsync();
#endif
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] OnResumeAppAsync: {ex}");
        }
    }

    private static async Task OnConnectivityAvailableAsync()
    {
        try
        {
            var ok = await NetConnectivity.HasInternetAsync();
            if (!ok || Analytics._isManualSessionEnabled) return;

            if (!await _connectivityGate.WaitAsync(0)) return;
            try
            {
                if (!AppAmbitSdk.InternalTokenIsValid())
                    await AppAmbitSdk.InternalEnsureToken(null);

                await SessionManager.SendEndSessionFromDatabase();
                await SessionManager.SendStartSessionIfExist();
                await Crashes.LoadCrashFileIfExists();
                await SessionManager.SendBatchSessions();

#if ANDROID
                await Analytics.SendBatchEvents();
                await Crashes.SendBatchLogs();
#else
                await SendAllPendingThrottledAsync();
#endif
                await BreadcrumbManager.SendPending();
            }
            finally
            {
                _connectivityGate.Release();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] OnConnectivityAvailableAsync: {ex}");
        }
    }

    private static async Task SendAllPendingThrottledAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            if (now - _lastSendAllAtUtc < _minSendInterval) return;
            _lastSendAllAtUtc = now;

            await SessionManager.SendBatchSessions();
            await Analytics.SendBatchEvents();
            await Crashes.SendBatchLogs();
            await BreadcrumbManager.SendPending();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MauiNativePlatforms] SendAllPendingThrottledAsync: {ex}");
        }
    }
}
