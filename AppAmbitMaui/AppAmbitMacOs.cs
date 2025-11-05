#if MACCATALYST
using System.Collections.Generic;
using Foundation;
using UIKit;

namespace AppAmbit;

internal static class AppAmbitMacOs
{
    static readonly List<NSObject> _obs = new();
    static bool _started;
    static bool _ever;
    static int _active;

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

    public static void Register(string appKey)
    {
        if (_started) return;
        _started = true;

        AppAmbitSdk.InternalStart(appKey);

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
