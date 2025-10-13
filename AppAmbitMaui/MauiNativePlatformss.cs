using System;
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

namespace AppAmbit;

internal static class MauiNativePlatformss
{
    private static string? _appKey;
    private static bool _hasStartedSession = false;
    private static bool _readyForBatches = false;

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

    private sealed class PauseRunnable : Java.Lang.Object, Java.Lang.IRunnable
    {
        public void Run()
        {
            if (_resumedActivities == 0 && _foreground && _isWaitingPause)
            {
                _foreground = false;
                Core.InternalSleep();
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
        _obsDidBecomeActive   = nc.AddObserver(UIApplication.DidBecomeActiveNotification, HandleDidBecomeActive);
        _obsWillResignActive  = nc.AddObserver(UIApplication.WillResignActiveNotification, HandleWillResignActive);
        _obsDidEnterBackground= nc.AddObserver(UIApplication.DidEnterBackgroundNotification, HandleDidEnterBackground);
        _obsWillEnterForeground=nc.AddObserver(UIApplication.WillEnterForegroundNotification, HandleWillEnterForeground);
        _obsWillTerminate     = nc.AddObserver(UIApplication.WillTerminateNotification, HandleWillTerminate);

        StartReachability();
        _ = OnStartAppAsync();
#endif
    }

#if ANDROID
    private sealed class LifecycleCallbacks : Java.Lang.Object, AApp.IActivityLifecycleCallbacks
    {
        public void OnActivityCreated(AActivity activity, ABundle? savedInstanceState) { }
        public void OnActivityStarted(AActivity activity)
        {
            if (_startedActivities == 0) { }
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
        }
        public void OnActivityPaused(AActivity activity)
        {
            _resumedActivities = Math.Max(0, _resumedActivities - 1);
            if (_resumedActivities == 0)
            {
                _isWaitingPause = true;
                _handler.PostDelayed(_pauseRunnable, _activityDelay);
            }
        }
        public void OnActivityStopped(AActivity activity)
        {
            _startedActivities = Math.Max(0, _startedActivities - 1);
            if (_startedActivities == 0 && !activity.IsChangingConfigurations)
            {
                Core.InternalEnd();
            }
        }
        public void OnActivitySaveInstanceState(AActivity activity, ABundle outState) { }
        public void OnActivityDestroyed(AActivity activity)
        {
            if (_startedActivities == 0 && _resumedActivities == 0 && !activity.IsChangingConfigurations)
            {
                Core.InternalEnd();
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
            var request = new NRequest.Builder().AddCapability(NCapability.Internet).Build();
            _netCallback ??= new NetCb();
            cm.RegisterNetworkCallback(request, _netCallback);
        }
        catch (global::Java.Lang.SecurityException)
        {
            _netCallback = null;
        }
        catch { }
    }

    private sealed class NetCb : CManager.NetworkCallback
    {
        public override void OnAvailable(ANetwork network)
        {
            base.OnAvailable(network);
            _handler.PostDelayed(new global::Java.Lang.Runnable(() => { _ = OnConnectivityAvailableAsync(); }), 3000);
        }
        public override void OnLost(ANetwork network) { base.OnLost(network); }
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
        if (IsReachable(flags))
        {
            _ = OnConnectivityAvailableAsync();
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

    private static void HandleDidBecomeActive(NSNotification n) { _ = OnResumeAppAsync(); }
    private static void HandleWillResignActive(NSNotification n) { Core.InternalSleep(); }
    private static void HandleDidEnterBackground(NSNotification n) { Core.InternalSleep(); }
    private static void HandleWillEnterForeground(NSNotification n) { _ = OnResumeAppAsync(); }
    private static void HandleWillTerminate(NSNotification n) { Core.InternalEnd(); }
#endif

    private static async Task OnStartAppAsync()
    {
        await Core.InternalStart(_appKey ?? string.Empty);
        _hasStartedSession = true;
        await Crashes.LoadCrashFileIfExists();
        await SessionManager.SendBatchSessions();
    }

    private static async Task OnResumeAppAsync()
    {
        if (Analytics._isManualSessionEnabled || !_readyForBatches)
        {
            _readyForBatches = true;
            return;
        }

        if (!Core.InternalTokenIsValid())
        {
            await Core.InternalEnsureToken(null);
        }

        if (!Analytics._isManualSessionEnabled)
        {
            await SessionManager.RemoveSavedEndSession();
        }

        await Crashes.LoadCrashFileIfExists();
        await Analytics.SendBatchEvents();
        await Crashes.SendBatchLogs();
    }

    private static async Task OnConnectivityAvailableAsync()
    {
        var ok = await NetConnectivity.HasInternetAsync();
        if (!ok || Analytics._isManualSessionEnabled) return;

        await Core.InternalEnsureToken(null);

        await Crashes.LoadCrashFileIfExists();
        await SessionManager.SendEndSessionFromDatabase();
        await SessionManager.SendStartSessionIfExist();
        await SessionManager.SendBatchSessions();

        await Analytics.SendBatchEvents();
        await Crashes.SendBatchLogs();
    }
}