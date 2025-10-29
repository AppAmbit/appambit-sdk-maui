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
    private static readonly SemaphoreSlim _connectivityGate = new(1, 1);

    private static readonly object _bcLock = new();
    private static string? _lastCrumb;
    private static DateTime _lastCrumbAtUtc = DateTime.MinValue;
    private static readonly TimeSpan _crumbWindow = TimeSpan.FromMilliseconds(400);

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
    private static bool? _androidOnline;
#endif

#if IOS
    private enum IOSAppState { Unknown, Foreground, Background, Terminated }
    private static IOSAppState _iosState = IOSAppState.Unknown;

    private static NSObject? _obsDidBecomeActive;
    private static NSObject? _obsWillResignActive;
    private static NSObject? _obsDidEnterBackground;
    private static NSObject? _obsWillEnterForeground;
    private static NSObject? _obsWillTerminate;
    private static NetworkReachability? _reachability;
    private static bool _iosPageBreadcrumbsEnabled = false;
    private static bool _iosPagesHooked = false;
    private static UINavigationControllerDelegate? _navDelegate;
    private static bool? _iosOnline;

    private sealed class NavDelegate : UINavigationControllerDelegate
    {
        public override void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            _ = AddCrumb("page_disappear");
        }
        public override void DidShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            _ = AddCrumb("page_appear");
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
        _ = AppAmbitSdk.InternalStart(_appKey ?? string.Empty);

#elif IOS
        if (_obsDidBecomeActive != null) return;

        var nc = NSNotificationCenter.DefaultCenter;
        _obsDidBecomeActive     = nc.AddObserver(UIApplication.DidBecomeActiveNotification, HandleDidBecomeActive);
        _obsWillResignActive    = nc.AddObserver(UIApplication.WillResignActiveNotification, HandleWillResignActive);
        _obsDidEnterBackground  = nc.AddObserver(UIApplication.DidEnterBackgroundNotification, HandleDidEnterBackground);
        _obsWillEnterForeground = nc.AddObserver(UIApplication.WillEnterForegroundNotification, HandleWillEnterForeground);
        _obsWillTerminate       = nc.AddObserver(UIApplication.WillTerminateNotification, HandleWillTerminate);

        _iosState = IOSAppState.Foreground;
        StartReachability();
        AppAmbitSdk.InternalStart(_appKey ?? string.Empty);
#endif
    }

    public static void EnableNativePageBreadcrumbs()
    {
#if ANDROID
        _androidPageBreadcrumbsEnabled = true;
#endif
#if IOS
        _iosPageBreadcrumbsEnabled = true;
        TryHookIOSNativePages();
#endif
    }

#if ANDROID
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
                _ = AppAmbitSdk.InternalResume();
            }

            if (_androidPageBreadcrumbsEnabled)
            {
                _ = AddCrumb("page_appear");
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
                _ = AddCrumb("page_disappear");
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
                AppAmbitSdk.InternalEnd();
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
            _androidOnline = true;
            _handler.PostDelayed(new Java.Lang.Runnable(() =>
            {
                _ = AddCrumb("online");
                _ = OnConnectivityAvailableAsync();
            }), 3000);
        }
        public override void OnLost(ANetwork network)
        {
            base.OnLost(network);
            if (_androidOnline != false)
            {
                _androidOnline = false;
                _ = AddCrumb("offline");
            }
        }
    }
#endif

#if IOS
    private static void StartReachability()
    {
        if (_reachability != null) return;

        _reachability = new NetworkReachability(new IPAddress(0));
        _reachability.SetNotification(OnReachabilityChanged);
        _reachability.Schedule(CFRunLoop.Current, CFRunLoop.ModeDefault);
    }

    private static void OnReachabilityChanged(NetworkReachabilityFlags flags)
    {
        var reachable = IsReachable(flags);
        if (reachable)
        {
            if (_iosOnline != true)
            {
                _iosOnline = true;
                _ = AddCrumb("online");
            }
            _ = OnConnectivityAvailableAsync();
        }
        else
        {
            if (_iosOnline != false)
            {
                _iosOnline = false;
                _ = AddCrumb("offline");
            }
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
        if (_iosPageBreadcrumbsEnabled && !_iosPagesHooked) TryHookIOSNativePages();

        if (_iosState != IOSAppState.Foreground)
        {
            _iosState = IOSAppState.Foreground;
            _ = AppAmbitSdk.InternalResume();
        }
    }

    private static void HandleWillResignActive(NSNotification n)
    {
        // No hacemos nada aquí para evitar duplicados; el "pause" se gestiona en DidEnterBackground.
    }

    private static void HandleDidEnterBackground(NSNotification n)
    {
        if (_iosState != IOSAppState.Background)
        {
            _iosState = IOSAppState.Background;
            AppAmbitSdk.InternalSleep();
        }
    }

    private static void HandleWillEnterForeground(NSNotification n)
    {
        if (_iosPageBreadcrumbsEnabled && !_iosPagesHooked) TryHookIOSNativePages();

        if (_iosState != IOSAppState.Foreground)
        {
            _iosState = IOSAppState.Foreground;
            _ = AppAmbitSdk.InternalResume();
        }
    }

    private static void HandleWillTerminate(NSNotification n)
    {
        if (_iosState != IOSAppState.Terminated)
        {
            _iosState = IOSAppState.Terminated;
            AppAmbitSdk.InternalEnd();
        }
    }

    private static void TryHookIOSNativePages()
    {
        if (_iosPagesHooked) return;

        foreach (var sceneObj in UIApplication.SharedApplication.ConnectedScenes)
        {
            if (sceneObj is UIWindowScene ws)
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

    private static async Task OnConnectivityAvailableAsync()
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

            await AppAmbitSdk.InternalSendPending();
        }
        finally
        {
            _connectivityGate.Release();
        }
    }

    private static Task AddCrumb(string name)
    {
        var now = DateTime.UtcNow;
        lock (_bcLock)
        {
            if (_lastCrumb == name && (now - _lastCrumbAtUtc) < _crumbWindow)
                return Task.CompletedTask;
            _lastCrumb = name;
            _lastCrumbAtUtc = now;
        }
        return BreadcrumbManager.AddAsync(name);
    }
}
