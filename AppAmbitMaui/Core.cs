using System.Net.Http.Headers;
using AppAmbit.Models.Analytics;
using AppAmbit.Models.App;
using AppAmbit.Models.Logs;
using AppAmbit.Models.Responses;
using AppAmbit.Services;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Microsoft.Maui.LifecycleEvents;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AppAmbit;

public static class Core
{
    private static bool _initialized;
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
                android.OnCreate((activity, state) =>
                {
                    Start(appKey);
                });
                android.OnResume(activity =>
                {
                    if (_initialized)
                        OnResume();
                });
                android.OnPause(activity =>
                {
                    OnSleep();
                });
            });
#elif IOS
            events.AddiOS(ios =>
            {
                ios.FinishedLaunching((application, options) =>
                {
                    Start(appKey);
                    return true;
                });
                ios.WillEnterForeground(application =>
                {
                    OnResume();
                });
                ios.DidEnterBackground(application =>
                {
                    OnSleep();
                });
            });
#endif
        });

        builder.Services.AddSingleton<IAPIService, APIService>();
        builder.Services.AddSingleton<IStorageService, StorageService>();
        builder.Services.AddSingleton<IAppInfoService, AppInfoService>();
        
        return builder;
    }

    private static async Task Start(string appKey)
    {
        await InitializeServices();

        await InitializeConsumer(appKey);

        if (!Analytics._isManualSessionEnabled)
        {
            await Analytics.StartSession();
        }
        
        _initialized = true;
        
        Crashes.SendBatchLogs();
    }

    private static async Task OnResume()
    {
        var appKey = await storageService?.GetAppId();
        await InitializeConsumer(appKey);
        
        if (!Analytics._isManualSessionEnabled)
        {
            await Analytics.StartSession();
        }
    }
    
    public static async Task OnSleep()
    {
        if (!Analytics._isManualSessionEnabled)
        {
            await Analytics.EndSession();
        }
    }

    private static async Task InitializeConsumer(string appKey = "")
    {
        var appId = await storageService.GetAppId();
        var deviceId = await storageService.GetDeviceId();
        var userId = await storageService.GetUserId();
        var userEmail = await storageService.GetUserEmail();

        if (appId == null)
        {
            await storageService.SetAppId(appKey);
        }

        if (deviceId == null)
        {
            var id = Guid.NewGuid().ToString();
            await storageService.SetDeviceId(id);
        }

        if (userId == null)
        {
            var id = Guid.NewGuid().ToString();
            await storageService.SetUserId(id);
        }

        var consumer = new Consumer
        {
            AppKey = appKey,
            DeviceId = deviceId,
            DeviceModel = appInfoService.DeviceModel,
            UserId = userId,
            UserEmail = userEmail,
            OS = appInfoService.OS,
            Country = appInfoService.Country,
            Language = appInfoService.Language,
        };
        var registerEndpoint = new RegisterEndpoint(consumer);
        var remoteToken = await apiService?.ExecuteRequest<TokenResponse>(registerEndpoint);

        apiService.SetToken(remoteToken?.Token);
    }
    
    private static async Task InitializeServices()
    {
        apiService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>();
        appInfoService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAppInfoService>();
        storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
        await storageService?.InitializeAsync();
        Crashes.Initialize(apiService,storageService);
        Analytics.Initialize(apiService,storageService);
    }
}