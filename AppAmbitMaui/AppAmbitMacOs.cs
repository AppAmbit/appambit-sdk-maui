#if MACCATALYST
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreFoundation;
using Foundation;
using Network;
using UIKit;

namespace AppAmbit;

internal static class AppAmbitMacOs
{
    static readonly List<NSObject> _obs = new();
    static bool _started;
    static bool _ever;
    static int _active;

    static NWPathMonitor _monitor;
    static bool _hasStatus;
    static NWPathStatus _lastStatus;
    static bool _isHandlingNetworkChange;

    static void TryResume()
    {
        if (_active <= 0) return;
        _ever = true;
        _ = AppAmbitSdk.InternalResume();
    }

    static void TrySleep()
    {
        if (!_ever) return;
        if (_active == 0) AppAmbitSdk.InternalSleep();
    }

    static void StartNetworkMonitoring()
    {
        if (_monitor != null) return;

        _monitor = new NWPathMonitor();
        var queue = new DispatchQueue("AppAmbitMacOs.NWPathMonitor");
        _monitor.SetQueue(queue);
        _monitor.SnapshotHandler = path => OnPathUpdated(path);
        _monitor.Start();
    }

    static void OnPathUpdated(NWPath path)
    {
        var status = path.Status;

        if (_hasStatus && status == _lastStatus) return;

        var previous = _hasStatus ? _lastStatus : status;
        _lastStatus = status;
        _hasStatus = true;

        if (status == NWPathStatus.Satisfied && previous != NWPathStatus.Satisfied)
        {
            _ = HandleNetworkChangeAsync();
        }
    }

    static async Task HandleNetworkChangeAsync()
    {
        if (_isHandlingNetworkChange) return;
        _isHandlingNetworkChange = true;

        try
        {
            if (!AppAmbitSdk.InternalTokenIsValid())
                await AppAmbitSdk.InternalEnsureToken(null);

            await SessionManager.SendEndSessionFromDatabase();
            await SessionManager.SendStartSessionIfExist();
            await Crashes.LoadCrashFileIfExists();
            await AppAmbitSdk.InternalSendPending();
        }
        finally
        {
            _isHandlingNetworkChange = false;
        }
    }

    public static void Register(string appKey)
    {
        if (_started) return;
        _started = true;

        AppAmbitSdk.InternalStart(appKey);
        StartNetworkMonitoring();

        var c = NSNotificationCenter.DefaultCenter;
        _obs.Add(c.AddObserver(UIScene.DidActivateNotification, _ => UIApplication.SharedApplication.InvokeOnMainThread(() => { _active++; TryResume(); })));
        _obs.Add(c.AddObserver(UIScene.WillDeactivateNotification, _ => UIApplication.SharedApplication.InvokeOnMainThread(() => { if (_active > 0) _active--; TrySleep(); })));
        _obs.Add(c.AddObserver(UIScene.DidEnterBackgroundNotification, _ => UIApplication.SharedApplication.InvokeOnMainThread(() => { if (_active > 0) _active--; TrySleep(); })));

        _active = 0;
        foreach (var s in UIApplication.SharedApplication.ConnectedScenes)
        {
            if (s is UIScene sc && sc.ActivationState == UISceneActivationState.ForegroundActive)
                _active++;
        }
        if (_active > 0) TryResume();
    }
}
#endif
