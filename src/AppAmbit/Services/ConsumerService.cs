using AppAmbit.Models.App;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using AppAmbit.Enums;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AppAmbit.Services;

internal class ConsumerService
{
    private static IStorageService? _storageService;
    private static IAPIService? _apiService;

    private static IAppInfoService? _appInfoService;

    public static void Initialize(IStorageService? storageService, IAppInfoService? appInfoService, IAPIService? apiService)
    {
        _storageService = storageService;
        _appInfoService = appInfoService;
        _apiService = apiService;
    }

    public static async Task<RegisterEndpoint> BuildRegisterEndpoint()
    {
        string? appId = null;
        string? deviceId = "", userId = "", userEmail = null;
        try
        {
            if (_storageService == null || _appInfoService == null || _apiService == null)
            {
                return new RegisterEndpoint(new Consumer());
            }

            deviceId = await _storageService.GetDeviceId();
            userId = await _storageService.GetUserId();
            userEmail = await _storageService.GetUserEmail();
            appId = await _storageService.GetAppId();

            if (string.IsNullOrWhiteSpace(deviceId))
            {
                deviceId = Guid.NewGuid().ToString();
                await _storageService.SetDeviceId(deviceId);
            }

            if (string.IsNullOrWhiteSpace(userId))
            {
                userId = Guid.NewGuid().ToString();
                await _storageService.SetUserId(userId);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ConsumerService] Error getting data for AppSecrets ConsumerService: {ex}");
        }

        return new RegisterEndpoint(new Consumer
        {
            AppKey = appId ?? "",
            DeviceId = deviceId ?? "",
            DeviceModel = _appInfoService?.DeviceModel ?? "",
            UserId = userId ?? "",
            UserEmail = userEmail,
            OS = _appInfoService?.OS ?? "",
            Country = _appInfoService?.Country ?? "",
            Language = _appInfoService?.Language ?? "",
            AppVersion = _appInfoService.AppVersion
        });
    }

    public static async Task<ApiErrorType> CreateConsumer()
    {
        try
        {
            if (_apiService == null || _storageService == null)
            {
                return ApiErrorType.Unknown;
            }

            var registerEndpoint = await BuildRegisterEndpoint();
            var tokenResponse = await _apiService.ExecuteRequest<TokenResponse>(registerEndpoint);

            if (tokenResponse == null)
            {
                Debug.Print($"[ConsumerService] Token Response is null");
            }

            if (tokenResponse?.ErrorType != ApiErrorType.None)
            {
                Debug.Print($"[ConsumerService] Request failed: {tokenResponse?.ErrorType}");
                return tokenResponse?.ErrorType ?? ApiErrorType.Unknown;
            }

            if (tokenResponse.Data == null)
            {
                Debug.Print("[ConsumerService] TokenResponse.Data is null");
                return ApiErrorType.Unknown;
            }

            _storageService?.SetConsumerId(tokenResponse.Data.ConsumerId);
            _apiService.SetToken(tokenResponse.Data.Token);

            return tokenResponse.ErrorType;
        }
        catch (Exception ex)
        {
            Debug.Print($"[ConsumerService] {ex.Message}");
            return ApiErrorType.Unknown;
        }
    }

    public static async Task UpdateAppKeyIfNeeded(string appKey)
    {
        if (_storageService == null)
        {
            return;
        }

        string newKey = (appKey ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(newKey))
        {
            return;
        }

        string? storedKey = await _storageService.GetAppId();
        if (string.Equals(storedKey, newKey, StringComparison.Ordinal))
        {
            return;
        }

        await _storageService.SetConsumerId(string.Empty);
        await _storageService.SetAppId(newKey);
    }

}