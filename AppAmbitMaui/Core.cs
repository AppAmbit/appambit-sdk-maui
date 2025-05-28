using System.Diagnostics;
using AppAmbit.Services;
using AppAmbit.Services.Interfaces;
using Microsoft.Maui.LifecycleEvents;

namespace AppAmbit;

public static class Core
{
    private static IAPIService? apiService;
    private static IStorageService? storageService;
    private static IAppInfoService? appInfoService;
    private static bool _hasStartedSession = false;

    public static MauiAppBuilder UseAppAmbit(this MauiAppBuilder builder, string appKey)
    {
        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
            events.AddAndroid(android =>
            {
                android.OnCreate((activity, state) => { OnStart(appKey); });
                android.OnPause(activity => { OnSleep(); });
                android.OnResume(activity => { OnResume(); });
                android.OnStop(activity => { OnSleep(); });
                android.OnRestart(activity => { OnResume(); });
                android.OnDestroy(activity => { OnEnd(); });
            });
#elif IOS
            events.AddiOS(ios =>
            {
                ios.FinishedLaunching((application, options) =>
                {
                    OnStart(appKey);
                    return true;
                });
                ios.DidEnterBackground(application => { OnSleep(); });
                ios.WillEnterForeground(application => { OnResume(); });
                ios.WillTerminate(application => { OnEnd(); });
            });
#endif
        });

        Connectivity.ConnectivityChanged -= OnConnectivityChanged;
        Connectivity.ConnectivityChanged += OnConnectivityChanged;
        Crashes.OnCrashException -= exception => { OnEnd(); };
        Crashes.OnCrashException += exception => { OnEnd(); };
        builder.Services.AddSingleton<IAPIService, APIService>();
        builder.Services.AddSingleton<IStorageService, StorageService>();
        builder.Services.AddSingleton<IAppInfoService, AppInfoService>();

        return builder;
    }


    private static async void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        Debug.WriteLine("OnConnectivityChanged");
        Debug.WriteLine($"NetworkAccess:{e.ToString()}");

        var access = e.NetworkAccess;

        if (access != NetworkAccess.Internet)
            return;

        await InitializeServices();

        if (!TokenIsValid())
            await apiService?.GetNewToken();


        await Crashes.LoadCrashFileIfExists();

        await Crashes.SendBatchLogs();
        await Analytics.SendBatchEvents();
    }

    private static async Task OnStart(string appKey)
    {
        await InitializeServices();

        await InitializeConsumer(appKey);
        _hasStartedSession = true;

        await Crashes.LoadCrashFileIfExists();

        await Crashes.SendBatchLogs();
        await Analytics.SendBatchEvents();
    }

    private static async Task OnResume()
    {
        if (!TokenIsValid())
            await apiService?.GetNewToken();


        if (!Analytics._isManualSessionEnabled && _hasStartedSession)
        {
            await SessionManager.RemoveSavedEndSession();
        }

        await Crashes.SendBatchLogs();
        await Analytics.SendBatchEvents();
    }

    private static void OnSleep()
    {
        if (!Analytics._isManualSessionEnabled)
        {
            SessionManager.SaveEndSession();
        }
    }

    private static void OnEnd()
    {
        if (!Analytics._isManualSessionEnabled)
        {
            SessionManager.SaveEndSession();
        }
    }

    private static async Task InitializeConsumer(string appKey)
    {
        await apiService?.GetNewToken(appKey);

        if (Analytics._isManualSessionEnabled)
            return;

        await SessionManager.SendEndSessionIfExists();
        await SessionManager.StartSession();
    }


    private static async Task InitializeServices()
    {
        apiService = apiService == null ? Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>() : apiService;
        appInfoService = appInfoService == null ? Application.Current?.Handler?.MauiContext?.Services.GetService<IAppInfoService>() : appInfoService;
        storageService = storageService == null ? Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>() : storageService;
        await storageService?.InitializeAsync();
        var deviceId = await storageService.GetDeviceId();
        SessionManager.Initialize(apiService);
        Crashes.Initialize(apiService, storageService, deviceId);
        Analytics.Initialize(apiService, storageService);
        ConsumerService.Initialize(storageService, appInfoService);
    }

    private static bool TokenIsValid()
    {
        var token = apiService?.GetToken();
        if (!string.IsNullOrEmpty(token))
            return true;
        return false;
    }

}