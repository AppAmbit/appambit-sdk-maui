using System.Diagnostics;
using Microsoft.Maui.LifecycleEvents;
using AppAmbit.Services;
using AppAmbit.Services.Interfaces;

namespace AppAmbit;

public static class AppAmbitMauiExtensions
{
    public static MauiAppBuilder UseAppAmbit(this MauiAppBuilder builder, string appKey)
    {
        AppAmbitSdk.MarkConfiguredByBuilder();

        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnCreate((activity, state) => { AppAmbitSdk.InternalStart(appKey); });
                android.OnPause(activity => { AppAmbitSdk.InternalSleep(); });
                android.OnResume(activity => { _ = AppAmbitSdk.InternalResume(); });
                android.OnStop(activity => { AppAmbitSdk.InternalSleep(); });
                android.OnRestart(activity => { _ = AppAmbitSdk.InternalResume(); });
                android.OnDestroy(activity => { AppAmbitSdk.InternalEnd(); });
            });
#elif IOS
            events.AddiOS(ios =>
            {
                ios.FinishedLaunching((application, options) =>
                {
                    AppAmbitSdk.InternalStart(appKey);
                    return true;
                });
                ios.DidEnterBackground(application => { AppAmbitSdk.InternalSleep(); });
                ios.WillEnterForeground(application => { _ = AppAmbitSdk.InternalResume(); });
                ios.WillTerminate(application => { AppAmbitSdk.InternalEnd(); });
            });
#endif
        });

        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
        Connectivity.ConnectivityChanged += OnConnectivityChanged;

        builder.Services.AddSingleton<IAPIService, APIService>();
        builder.Services.AddSingleton<IStorageService, StorageService>();
        builder.Services.AddSingleton<IAppInfoService, AppInfoService>();

        return builder;
    }

    private static async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        Debug.WriteLine("OnConnectivityChanged");
        if (e.NetworkAccess != NetworkAccess.Internet) return;

        if (!AppAmbitSdk.InternalTokenIsValid())
            await AppAmbitSdk.InternalEnsureToken(null);

        await SessionManager.SendEndSessionFromDatabase();
        await SessionManager.SendStartSessionIfExist();
        await Crashes.LoadCrashFileIfExists();
        await AppAmbitSdk.InternalSendPending();
    }
}
