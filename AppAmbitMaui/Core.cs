using System.Diagnostics;
using AppAmbit.Enums;
using AppAmbit.Models.Responses;
using AppAmbit.Services;
using AppAmbit.Services.Interfaces;
using Microsoft.Maui.LifecycleEvents;

namespace AppAmbit;

public static class Core
{
    private static IAPIService? apiService;
    private static IStorageService? storageService;
    private static IAppInfoService? appInfoService;
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
                android.OnDestroy(async activity1 => { await OnEnd(); });
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
                ios.WillTerminate(async application => { await  OnEnd(); });
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

    private static async Task OnStart(string appKey)
    {
        await InitializeServices();
        
        await InitializeConsumer(appKey);

        await Crashes.LoadCrashFileIfExists();
        
        await Crashes.SendBatchLogs();
        await Analytics.SendBatchEvents();
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
            await InitializeConsumer();

        await Crashes.LoadCrashFileIfExists();

        await Crashes.SendBatchLogs();
        await Analytics.SendBatchEvents();
    }

    private static bool TokenIsValid()
    {
        var token = apiService?.GetToken();
        if (!string.IsNullOrEmpty(token))
            return true;
        return false;
    }

    private static async Task OnResume()
    {
        await InitializeServices();

        if (!TokenIsValid())
            await InitializeConsumer();

        if (!Analytics._isManualSessionEnabled)
        {
            await SessionManager.RemoveSavedEndSession();
        }

        await Crashes.SendBatchLogs();
        await Analytics.SendBatchEvents();
    }

    private static async Task OnSleep()
    {
        if (!Analytics._isManualSessionEnabled)
        {
            await SessionManager.SaveEndSession();
        }
    }

    private static async Task OnEnd()
    {
        if (!Analytics._isManualSessionEnabled)
        {
            await SessionManager.SaveEndSession();
        }
    }

    private static async Task InitializeConsumer(string appKey = "")
    {
        await apiService?.GetNewToken(appKey);

        if (!Analytics._isManualSessionEnabled)
        {
            await SessionManager.SendEndSessionIfExists();
            await SessionManager.StartSession();
        }
    }


    private static async Task InitializeServices()
    {
        apiService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>();
        appInfoService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAppInfoService>();
        storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
        await storageService?.InitializeAsync();
        var deviceId = await storageService.GetDeviceId();
        SessionManager.Initialize(apiService, storageService);
        Crashes.Initialize(apiService, storageService, deviceId);
        Analytics.Initialize(apiService, storageService);
        ConsumerService.Initialize(storageService, appInfoService);
    }

}