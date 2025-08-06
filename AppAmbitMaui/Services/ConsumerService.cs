using AppAmbit.Models.App;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using AppAmbit.Enums;
using System.Diagnostics;

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

    public static async Task<RegisterEndpoint> BuildRegisterEndpoint(string? appKey)
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
            var storedAppKey = await _storageService.GetAppId();


            if (storedAppKey != appKey)
            {
                await _storageService.SetConsumerId("");
            }

            if (!string.IsNullOrWhiteSpace(appKey))
            {
                appId = appKey;
                await _storageService.SetAppId(appKey);
            }

            if (string.IsNullOrWhiteSpace(appKey))
            {
                appId = storedAppKey ?? "";
            }

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
        });
    }

    public static async Task<ApiErrorType> CreateConsumer(string appKey)
    {
        try
        {
            if (_apiService == null || _storageService == null)
            {
                return ApiErrorType.Unknown;
            }

            var registerEndpoint = await BuildRegisterEndpoint(appKey);
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
}