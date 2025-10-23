using System.Diagnostics;
using AppAmbit.Services;
using AppAmbit.Services.Interfaces;
using Microsoft.Maui.LifecycleEvents;

namespace AppAmbit;

public static class AppAmbitSdk
{
    private static IAPIService? apiService;
    private static IStorageService? storageService;
    private static IAppInfoService? appInfoService;
    private static bool _hasStartedSession = false;
    private static readonly SemaphoreSlim consumerSemaphore = new(1, 1);
    private static readonly SemaphoreSlim _ensureBatchLocked = new(1, 1);
    private static bool _configuredByBuilder = false;

    private static readonly object _initLock = new();
    private static bool _servicesReady = false;

    private static bool _pageEventsHooked = false;

    public static MauiAppBuilder UseAppAmbit(this MauiAppBuilder builder, string appKey)
    {
        _configuredByBuilder = true;

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
        Debug.WriteLine($"NetworkAccess:{e}");

        var access = e.NetworkAccess;

        if (access != NetworkAccess.Internet)
        {
            await BreadcrumbManager.AddAsync("offline");
            return;
        }

        if (!TokenIsValid())
        {
            await GetNewToken(null);
        }

        await BreadcrumbManager.AddAsync("offline");

        await BreadcrumbManager.AddAsync("online");

        await SessionManager.SendEndSessionFromDatabase();
        await SessionManager.SendStartSessionIfExist();
        await Crashes.LoadCrashFileIfExists();
        await SendDataPending();
        
    }    

    private static async Task OnStart(string appKey)
    {
        if (_hasStartedSession) return;

        await InitializeServices();
        await InitializeConsumer(appKey);
        _hasStartedSession = true;

        HookPageEvents();

        await BreadcrumbManager.AddAsync("app_start");

        await Crashes.LoadCrashFileIfExists();
        await SendDataPending();
    }

    private static async Task OnResume()
    {
        if (!_servicesReady)
        {
            try { await InitializeServices(); } catch { return; }
            if (!_servicesReady) return;
        }

        if (!TokenIsValid())
        {
            await GetNewToken(null);
        }

        if (!Analytics._isManualSessionEnabled && _hasStartedSession)
        {
            await SessionManager.RemoveSavedEndSession();
        }

        await BreadcrumbManager.AddAsync("app_resume");

        await Crashes.LoadCrashFileIfExists();
        await SendDataPending();
    }

    private static void OnSleep()
    {
        _ = BreadcrumbManager.AddAsync("app_pause");

        if (!Analytics._isManualSessionEnabled)
        {
            SessionManager.SaveEndSession();
        }
    }

    private static void OnEnd()
    {
        _ = BreadcrumbManager.AddAsync("app_destroy");

        if (!Analytics._isManualSessionEnabled)
        {
            SessionManager.SaveEndSession();
        }
    }

    private static async Task InitializeConsumer(string appKey)
    {
        if (!Analytics._isManualSessionEnabled)
        {
            await SessionManager.SaveSessionEndToDatabaseIfExist();
        }

        await GetNewToken(appKey);

        if (Analytics._isManualSessionEnabled)
        {
            return;
        }

        await SessionManager.SendEndSessionFromDatabase();
        await SessionManager.SendEndSessionFromFile();
        await SessionManager.StartSession();
    }

    private static async Task SendDataPending()
    {
        await _ensureBatchLocked.WaitAsync();
        try
        {
            await SessionManager.SendBatchSessions();
            await Crashes.SendBatchLogs();
            await Analytics.SendBatchEvents();
            await BreadcrumbManager.SendPending();
        }
        finally
        {
            _ensureBatchLocked.Release();
        }
    }

    private static async Task InitializeServices()
    {
        if (_servicesReady) return;

        try
        {
            lock (_initLock)
            {
                apiService     ??= new APIService();
                appInfoService ??= new AppInfoService();
                storageService ??= new StorageService();
            }

            TokenService.Initialize(storageService);
            await storageService!.InitializeAsync();
            var deviceId = await storageService.GetDeviceId();
            SessionManager.Initialize(apiService, storageService);
            Crashes.Initialize(apiService, storageService, deviceId ?? "");
            Analytics.Initialize(apiService, storageService);
            ConsumerService.Initialize(storageService, appInfoService, apiService);

            // Breadcrumbs
            BreadcrumbManager.Initialize(apiService!, storageService!);

            _servicesReady = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
            _servicesReady = false;
            throw;
        }
    }

    private static async Task GetNewToken(string? appKey)
    {
        await consumerSemaphore.WaitAsync();
        try
        {
            if (!_servicesReady) return;
            if (TokenIsValid()) return;

            if (storageService == null) return;

            await ConsumerService.UpdateAppKeyIfNeeded(appKey);
            var consumerId = await storageService.GetConsumerId();
            if (!string.IsNullOrWhiteSpace(consumerId))
            {
                var result = await apiService!.GetNewToken();
            }
            else
            {
                var result = await ConsumerService.CreateConsumer();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AppAmbitSdk] Exception during token operation: {ex}");
        }
        finally
        {
            consumerSemaphore.Release();
        }
    }

    private static bool TokenIsValid()
    {
        var token = apiService?.GetToken();
        if (!string.IsNullOrEmpty(token))
            return true;
        return false;
    }

    public static void Start(string appKey)
    {
        if (_configuredByBuilder) return;
        HookPlatformLifecycle(appKey);
    }

    private static void HookPlatformLifecycle(string appKey)
    {
        MauiNativePlatforms.Register(appKey);
    }

    internal static Task InternalStart(string appKey) => OnStart(appKey);
    internal static Task InternalResume() => OnResume();
    internal static void InternalSleep() => OnSleep();
    internal static void InternalEnd() => OnEnd();
    internal static Task InternalEnsureToken(string? appKey) => GetNewToken(appKey);
    internal static Task InternalSendPending() => SendDataPending();
    internal static bool InternalTokenIsValid() => TokenIsValid();

    private static void HookPageEvents()
    {
        MauiNativePlatforms.EnableNativePageBreadcrumbs();
    }
}