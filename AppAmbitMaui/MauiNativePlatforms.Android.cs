#if ANDROID
using System;
using System.Threading;
using Android.Content;
using Android.Net;
using Android.Views;
using AppAmbit.Services;
using System.Diagnostics;

using AApp = Android.App.Application;
using AActivity = Android.App.Activity;
using ABundle = Android.OS.Bundle;
using AHandler = Android.OS.Handler;
using ALooper = Android.OS.Looper;
using AContext = Android.Content.Context;
using CManager = Android.Net.ConnectivityManager;
using NRequest = Android.Net.NetworkRequest;
using NCapability = Android.Net.NetCapability;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace AppAmbit;

internal static partial class MauiNativePlatforms
{
    private static volatile bool _androidInitialized;
    private static int _startedActivities;
    private static int _resumedActivities;
    private static volatile bool _inForeground;
    private static volatile bool _waitingPause;

    private static volatile bool _androidPageBreadcrumbsEnabled;
    private static readonly object _pageLock = new();
    private static string? _currentPageKey;

    private static CManager.NetworkCallback? _netCallback;

    private static readonly long _activityDelayMs = 700;
    private static readonly AHandler _handler = new(ALooper.MainLooper);
    private static readonly Java.Lang.IRunnable _pauseRunnable = new PauseRunnable();

    static partial void PlatformRegister(string appKey)
    {
        if (_androidInitialized) return;
        var app = AApp.Context as AApp;
        if (app is null) return;

        app.RegisterActivityLifecycleCallbacks(new LifecycleCallbacks());
        TryRegisterNetworkCallback(AApp.Context);
        _androidInitialized = true;
    }

    static partial void PlatformEnablePageBreadcrumbs()
    {
        _androidPageBreadcrumbsEnabled = true;
    }

    private static void EmitKey(string key)
    {
        string? prev;
        lock (_pageLock)
        {
            if (_currentPageKey == key) return;
            prev = _currentPageKey;
            _currentPageKey = key;
        }
        if (prev != null) _ = AddCrumb(BreadcrumbsConstants.onDisappear);
        _ = AddCrumb(BreadcrumbsConstants.onAppear);
    }

    private static string KeyFromView(Context ctx, AView? v)
    {
        if (v is null) return "view:Unknown";
        if (v.Id != AView.NoId)
        {
            try
            {
                var name = ctx.Resources?.GetResourceEntryName(v.Id);
                if (!string.IsNullOrWhiteSpace(name)) return $"view:{name}";
            }
            catch { }
        }
        return $"view:{v.GetType().FullName}";
    }

    private static string KeyFromActivity(AActivity activity)
    {
        var root = activity.FindViewById<AViewGroup>(Android.Resource.Id.Content);
        if (root != null && root.ChildCount > 0)
        {
            var child = root.GetChildAt(root.ChildCount - 1);
            return KeyFromView(activity, child);
        }
        return $"activity:{activity.GetType().FullName}";
    }

    private sealed class PauseRunnable : Java.Lang.Object, Java.Lang.IRunnable
    {
        public void Run()
        {
            if (_resumedActivities == 0 && _inForeground && _waitingPause)
            {
                _inForeground = false;
                AppAmbitSdk.InternalSleep();
            }
            _waitingPause = false;
        }
    }

    private sealed class LifecycleCallbacks : Java.Lang.Object, AApp.IActivityLifecycleCallbacks
    {
        public void OnActivityCreated(AActivity activity, ABundle? savedInstanceState)
        {
            if (!_androidPageBreadcrumbsEnabled) return;

            var root = activity.FindViewById<AViewGroup>(Android.Resource.Id.Content);
            if (root != null)
            {
                root.SetOnHierarchyChangeListener(new RootHierarchyListener(activity));
            }
        }

        public void OnActivityStarted(AActivity activity)
        {
            Interlocked.Increment(ref _startedActivities);
        }

        public void OnActivityResumed(AActivity activity)
        {
            Interlocked.Increment(ref _resumedActivities);

            if (_waitingPause)
            {
                _handler.RemoveCallbacks(_pauseRunnable);
                _waitingPause = false;
            }

            if (!_inForeground)
            {
                _inForeground = true;
                _ = AppAmbitSdk.InternalResume();
            }

            if (_androidPageBreadcrumbsEnabled)
            {
                EmitKey(KeyFromActivity(activity));
            }
        }

        public void OnActivityPaused(AActivity activity)
        {
            InterlockedExtensions.DecrementToZero(ref _resumedActivities);

            if (_resumedActivities == 0)
            {
                _waitingPause = true;
                _handler.PostDelayed(_pauseRunnable, _activityDelayMs);
            }
        }

        public void OnActivityStopped(AActivity activity)
        {
            InterlockedExtensions.DecrementToZero(ref _startedActivities);
            if (_startedActivities == 0 && !activity.IsChangingConfigurations)
                AppAmbitSdk.InternalSleep();
        }

        public void OnActivitySaveInstanceState(AActivity activity, ABundle outState) { }

        public void OnActivityDestroyed(AActivity activity)
        {
            if (_startedActivities == 0 && _resumedActivities == 0 && !activity.IsChangingConfigurations)
                AppAmbitSdk.InternalEnd();
        }
    }

    private sealed class RootHierarchyListener : Java.Lang.Object, AViewGroup.IOnHierarchyChangeListener
    {
        private readonly WeakReference<AActivity> _actRef;

        public RootHierarchyListener(AActivity activity)
        {
            _actRef = new WeakReference<AActivity>(activity);
        }

        public void OnChildViewAdded(AView? parent, AView? child)
        {
            if (!_androidPageBreadcrumbsEnabled || child is null) return;
            if (_actRef.TryGetTarget(out var act))
            {
                EmitKey(KeyFromView(act, child));
            }
        }

        public void OnChildViewRemoved(AView? parent, AView? child) { }
    }

    private static void TryRegisterNetworkCallback(AContext context)
    {
        var cm = (CManager?)context.GetSystemService(AContext.ConnectivityService);
        if (cm is null) return;

        try
        {
            var request = new NRequest.Builder().AddCapability(NCapability.Internet).Build();
            _netCallback ??= new NetCb();
            cm.RegisterNetworkCallback(request, _netCallback);
        }
        catch (Java.Lang.SecurityException)
        {
            _netCallback = null;
        }
        catch
        {
            Debug.WriteLine("Error in TryRegisterNetworkCallback");
        }
    }

    private sealed class NetCb : CManager.NetworkCallback
    {
        public override void OnAvailable(Android.Net.Network network)
        {
            base.OnAvailable(network);
            _handler.PostDelayed(new Java.Lang.Runnable(async () =>
            {
                await AddCrumb(BreadcrumbsConstants.online);
                await OnConnectivityAvailableAsync();
            }), 3000);
        }

        public override void OnLost(Android.Net.Network network)
        {
            base.OnLost(network);
            AddCrumbFile(BreadcrumbsConstants.offline);
        }
    }

    private static class InterlockedExtensions
    {
        public static void DecrementToZero(ref int target)
        {
            int initial, computed;
            do
            {
                initial = Volatile.Read(ref target);
                computed = Math.Max(0, initial - 1);
            } while (Interlocked.CompareExchange(ref target, computed, initial) != initial);
        }
    }
}
#endif
