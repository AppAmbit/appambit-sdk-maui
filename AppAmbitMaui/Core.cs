using System.Diagnostics;
using AppAmbit.Models.App;
using AppAmbit.Models.Responses;
using AppAmbit.Services;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Microsoft.Maui.LifecycleEvents;

namespace AppAmbit;

public static class Core
{
    private static IAPIService? apiService;
    private static IStorageService? storageService;
    private static IAppInfoService? appInfoService;
    private static bool _testing = true;
    
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

        Crashes.LoadCrashFileIfExists();
        
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

        if (!TokenIsValid())
            await InitializeConsumer();

        Crashes.LoadCrashFileIfExists();

        await Crashes.SendBatchLogs();
        await Analytics.SendBatchEvents();
    }

    private static bool TokenIsValid()
    {
        var token = apiService?.GetToken();
        if( !string.IsNullOrEmpty(token) )
            return true;
        return false;
    }

    private static async Task OnResume()
    {
        if (!TokenIsValid())
            await InitializeConsumer();
        
        if (!Analytics._isManualSessionEnabled)
        {
            await Analytics.RemoveSavedEndSession();
        }
        
        await Crashes.SendBatchLogs();
        await Analytics.SendBatchEvents();
    }
    
    private static async Task OnSleep()
    {
        if (!Analytics._isManualSessionEnabled)
        {
            await Analytics.SaveEndSession();
        }
    }
    
    private static async Task OnEnd()
    {
        if (!Analytics._isManualSessionEnabled)
        {
            await Analytics.SaveEndSession();
        }
    }
    // The access modifier is internal because this method
    // is used to refresh the token when it has expired.
    internal static async Task InitializeConsumer(string appKey = "")
    {
        string appId = "";     
        var deviceId = await storageService.GetDeviceId();
        var userId = await storageService.GetUserId();
        var userEmail = await storageService.GetUserEmail();

        if (!string.IsNullOrEmpty(appKey))
        {
            appId = appKey;
            await storageService.SetAppId(appKey);
        }

        if (string.IsNullOrEmpty(appKey))
        {
            appId = await storageService.GetAppId() ?? "";
        }

        if (deviceId == null)
        {
            deviceId = Guid.NewGuid().ToString();
            await storageService.SetDeviceId(deviceId);
        }

        if (userId == null)
        {
            userId = Guid.NewGuid().ToString();
            await storageService.SetUserId(userId);
        }

        var consumer = new Consumer
        {
            AppKey = appId,
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
        if (remoteToken == null)
            return;
        
        // This is just for a test
        if (_testing)
        {
            remoteToken.Token = "662|eSaTdJ2qCHpo5lkyBMkTBopyuYqFCyKzbqH5Zwex2db9da1b";
            _testing = false;
        }

        apiService.SetToken(remoteToken?.Token);

        if (!Analytics._isManualSessionEnabled)
        {
            Analytics.SendEndSessionIfExists();
            await Analytics.StartSession();
        }
    }

    private static async Task InitializeServices()
    {
        apiService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>();
        appInfoService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAppInfoService>();
        storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
        await storageService?.InitializeAsync();
        var deviceId = await storageService.GetDeviceId();
        Crashes.Initialize(apiService,storageService,deviceId);
        Analytics.Initialize(apiService,storageService);
    }
}