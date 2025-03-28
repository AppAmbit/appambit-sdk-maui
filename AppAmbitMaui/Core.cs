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
        
        await StartSession();

        var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        if (hasInternet)
        {
            await SendAnalytics();
        }
        
        _initialized = true;
    }

    private static async Task OnResume()
    {
        var appKey = await storageService?.GetAppId();
        await InitializeConsumer(appKey);
        await StartSession();

    }
    
    public static async Task OnSleep()
    {
        await EndSession();
    }

    private static async Task InitializeConsumer(string appKey)
    {
        var appId = await storageService?.GetAppId();
        var deviceId = await storageService.GetDeviceId();

        if (appId == null)
        {
            await storageService.SetAppId(appKey);
        }

        if (deviceId == null)
        {
            var id = Guid.NewGuid().ToString();
            await storageService.SetDeviceId(id);
        }

        var consumer = new Consumer
        {
            AppKey = appKey,
            DeviceId = await storageService.GetDeviceId(),
            DeviceModel = appInfoService.DeviceModel,
            UserId = Guid.NewGuid().ToString(),
            IsGuest = true,
            UserEmail = "test@gmail.com",
            OS = appInfoService.OS,
            Country = appInfoService.Country,
            Language = appInfoService.Language,
        };
        var registerEndpoint = new RegisterEndpoint(consumer);
        var remoteToken = await apiService?.ExecuteRequest<TokenResponse>(registerEndpoint);

        await storageService.SetToken(remoteToken?.Token);
    }

    private static async Task StartSession()
    {
        var response = await apiService?.ExecuteRequest<SessionResponse>(new StartSessionEndpoint());
        storageService?.SetSessionId(response.SessionId);
    }

    private static async Task EndSession()
    {
        var sessionId = await storageService?.GetSessionId();
        await apiService?.ExecuteRequest<string>(new EndSessionEndpoint(sessionId));
    }

    private static async Task SendAnalytics()
    {
        var analytics = await storageService?.GetAllAnalyticsAsync();
        if (analytics.Count == 0)
        {
            return;
        }
        
        foreach (var item in analytics)
        {
            var analyticsReport = new Models.Analytics.AnalyticsReport()
            {
                EventTitle = item.EventTitle,
                SessionId = await storageService.GetSessionId(),
                Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(item.Data)
            };
            var result = await apiService.ExecuteRequest<object>(new SendAnalyticsEndpoint(analyticsReport));
        }
    }
        
    private static async Task InitializeServices()
    {
        apiService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>();
        appInfoService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAppInfoService>();
        storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
        await storageService?.InitializeAsync();
    }
    
}