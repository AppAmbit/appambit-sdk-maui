#if IOS
using System;
using System.Net;
using CoreFoundation;
using Foundation;
using SystemConfiguration;
using UIKit;
using AppAmbit.Services;

namespace AppAmbit;

internal static partial class MauiNativePlatforms
{
    private static NSObject? _obsDidBecomeActive;
    private static NSObject? _obsDidEnterBackground;
    private static NSObject? _obsWillTerminate;

    private static NetworkReachability? _reachability;
    private static bool _pagesHooked;
    private static UINavigationControllerDelegate? _navDelegate;

    static partial void PlatformRegister(string appKey)
    {
        if (_obsDidBecomeActive != null) return;

        var nc = NSNotificationCenter.DefaultCenter;
        _obsDidBecomeActive    = nc.AddObserver(UIApplication.DidBecomeActiveNotification, _ => AppAmbitSdk.InternalResume());
        _obsDidEnterBackground = nc.AddObserver(UIApplication.DidEnterBackgroundNotification, _ => AppAmbitSdk.InternalSleep());
        _obsWillTerminate      = nc.AddObserver(UIApplication.WillTerminateNotification, _ => AppAmbitSdk.InternalEnd());

        DispatchQueue.MainQueue.DispatchAsync(StartReachability);
    }

    static partial void PlatformEnablePageBreadcrumbs()
    {
        DispatchQueue.MainQueue.DispatchAsync(TryHookIOSNativePages);
    }

    private static void StartReachability()
    {
        try
        {
            _reachability?.Unschedule(CFRunLoop.Main, CFRunLoop.ModeDefault);
            _reachability = new NetworkReachability(new IPAddress(0));
            _reachability.SetNotification(OnReachabilityChanged);
            _reachability.Schedule(CFRunLoop.Main, CFRunLoop.ModeDefault);

            if (_reachability.TryGetFlags(out var flags))
                OnReachabilityChanged(flags);
        }
        catch(Exception ex)
        {
            Debug.WriteLine(ex.Message)
        }
    }

    private static void OnReachabilityChanged(NetworkReachabilityFlags flags)
    {
        if (IsReachable(flags))
        {
            _ = AddCrumb(BreadcrumbsConstants.online);
            _ = OnConnectivityAvailableAsync();
        }
        else
        {
            AddCrumbFile(BreadcrumbsConstants.offline);
        }
    }

    private static bool IsReachable(NetworkReachabilityFlags flags)
    {
        var reachable = flags.HasFlag(NetworkReachabilityFlags.Reachable);
        var noConnReq = !flags.HasFlag(NetworkReachabilityFlags.ConnectionRequired);
        var autoConn  = flags.HasFlag(NetworkReachabilityFlags.ConnectionOnDemand) ||
                        flags.HasFlag(NetworkReachabilityFlags.ConnectionOnTraffic);
        var noUser    = !flags.HasFlag(NetworkReachabilityFlags.InterventionRequired);
        return reachable && (noConnReq || (autoConn && noUser));
    }

    private static void TryHookIOSNativePages()
    {
        if (_pagesHooked) return;

        var app = UIApplication.SharedApplication;
        if (app?.ConnectedScenes == null) return;

        foreach (var scene in app.ConnectedScenes)
        {
            if (scene is not UIWindowScene ws) continue;
            foreach (var window in ws.Windows)
            {
                var nav = FindNavController(window.RootViewController);
                if (nav == null) continue;

                _navDelegate ??= new NavDelegate();
                nav.Delegate = _navDelegate;
                _pagesHooked = true;
                return;
            }
        }
    }

    private static UINavigationController? FindNavController(UIViewController? vc)
    {
        if (vc == null) return null;
        if (vc is UINavigationController nvc) return nvc;
        if (vc.PresentedViewController != null)
            return FindNavController(vc.PresentedViewController);

        if (vc is UITabBarController tab && tab.SelectedViewController != null)
            return FindNavController(tab.SelectedViewController);

        foreach (var child in vc.ChildViewControllers)
        {
            var result = FindNavController(child);
            if (result != null) return result;
        }
        return null;
    }

    private sealed class NavDelegate : UINavigationControllerDelegate
    {
        private string? _currentPage;

        public override void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            if (_currentPage != null)
                _ = AddCrumb(BreadcrumbsConstants.onDisappear);
        }

        public override void DidShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            var newPage = viewController?.GetType()?.FullName ?? "UnknownVC";
            if (_currentPage != newPage)
            {
                _currentPage = newPage;
                _ = AddCrumb(BreadcrumbsConstants.onAppear);
            }
        }
    }
}
#endif
