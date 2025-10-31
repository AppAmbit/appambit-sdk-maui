using System.Diagnostics;
using AppAmbit.Services;
using AppAmbit.Services.Interfaces;

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
    private static bool _servicesReady = false;
    public static void MarkConfiguredByBuilder() => _configuredByBuilder = true;

    private static void OnStart(string appKey)
    {
        try
        {
            if (_hasStartedSession) return;

            InitializeServices();
            InitializeConsumer(appKey);

            AsyncHelpers.RunSync(() => BreadcrumbManager.AddAsync(BreadcrumbsConstants.onStart));

            _hasStartedSession = true;
            BreadcrumbManager.LoadBreadcrumbsFromFile();
            AsyncHelpers.RunSync(() => Crashes.LoadCrashFileIfExists());
            AsyncHelpers.RunSync(SendDataPending);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AppAmbitSdk] Exception during OnStart initialization: {ex}");
        }
    }

    private static async Task OnResume()
    {
        if (!_servicesReady)
        {
            InitializeServices();
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
        
        BreadcrumbManager.LoadBreadcrumbsFromFile();
        await BreadcrumbManager.AddAsync(BreadcrumbsConstants.onResume);        
        await Crashes.LoadCrashFileIfExists();
        await SendDataPending();
    }

    private static void OnSleep()
    {

        if (!Analytics._isManualSessionEnabled)
        {
            BreadcrumbManager.SaveFile(BreadcrumbsConstants.onPause);
            SessionManager.SaveEndSession();
        }
    }

    private static void OnEnd()
    {
        if (!Analytics._isManualSessionEnabled)
        {
            BreadcrumbManager.SaveFile(BreadcrumbsConstants.onDestroy);
            SessionManager.SaveEndSession();
        }
    }

    private static void InitializeConsumer(string appKey)
    {
        if (!Analytics._isManualSessionEnabled)
        {
            AsyncHelpers.RunSync(SessionManager.SaveSessionEndToDatabaseIfExist);
        }

        AsyncHelpers.RunSync(() => GetNewToken(appKey));

        if (Analytics._isManualSessionEnabled)
            return;

        AsyncHelpers.RunSync(SessionManager.SendEndSessionFromDatabase);
        AsyncHelpers.RunSync(SessionManager.SendEndSessionFromFile);
        AsyncHelpers.RunSync(SessionManager.StartSession);
    }

    private static async Task SendDataPending()
    {
        await _ensureBatchLocked.WaitAsync();
        try
        {
            await SessionManager.SendBatchSessions();
            await Crashes.SendBatchLogs();
            await Analytics.SendBatchEvents();
            await BreadcrumbManager.SendBatchBreadcrumbs();
        }
        finally
        {
            _ensureBatchLocked.Release();
        }
    }

    private static void InitializeServices()
    {
        if (_servicesReady) return;

        try
        {
            apiService ??= new APIService();
            appInfoService ??= new AppInfoService();
            storageService ??= new StorageService();

            AsyncHelpers.RunSync(() => storageService!.InitializeAsync());
            ConsumerService.Initialize(storageService, appInfoService, apiService);
            TokenService.Initialize(storageService);

            var deviceId = AsyncHelpers.RunSync(() => storageService.GetDeviceId());

            SessionManager.Initialize(apiService, storageService);
            Crashes.Initialize(apiService, storageService, deviceId ?? "");
            Analytics.Initialize(apiService, storageService);
            ConsumerService.Initialize(storageService, appInfoService, apiService);

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
                _ = await apiService!.GetNewToken();
            else
                _ = await ConsumerService.CreateConsumer();
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
        return !string.IsNullOrEmpty(token);
    }

    public static void Start(string appKey)
    {
        try
        {
            if (_configuredByBuilder) return;
            HookPlatformLifecycle(appKey);            
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Erro to start: {ex.Message}");
        }
    }

    private static void HookPlatformLifecycle(string appKey)
    {
        try
        {
            MauiNativePlatforms.Register(appKey);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error to start: {ex.Message}");             
        }
    }

    internal static void InternalStart(string appKey) => OnStart(appKey);
    internal static Task InternalResume() => OnResume();
    internal static void InternalSleep() => OnSleep();
    internal static void InternalEnd() => OnEnd();
    internal static Task InternalEnsureToken(string? appKey) => GetNewToken(appKey);
    internal static Task InternalSendPending() => SendDataPending();
    internal static bool InternalTokenIsValid() => TokenIsValid();
}