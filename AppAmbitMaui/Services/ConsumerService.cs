using AppAmbit.Models.App;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using AppAmbit.Enums;

namespace AppAmbit.Services;

internal class ConsumerService
{
    private static IStorageService? _storageService;
    private static IAppInfoService? _appInfoService;

    public static void Initialize(IStorageService? storageService, IAppInfoService? appInfoService)
    {
        _storageService = storageService;
        _appInfoService = appInfoService;
    }

    public static async Task<RegisterEndpoint> RegisterConsumer(string appKey = "")
    {
        string appId = "";
        var deviceId = await _storageService.GetDeviceId();
        var userId = await _storageService.GetUserId();
        var userEmail = await _storageService.GetUserEmail();

        if (!string.IsNullOrEmpty(appKey))
        {
            appId = appKey;
            await _storageService.SetAppId(appKey);
        }

        if (string.IsNullOrEmpty(appKey))
        {
            appId = await _storageService.GetAppId() ?? "";
        }

        if (deviceId == null)
        {
            deviceId = Guid.NewGuid().ToString();
            await _storageService.SetDeviceId(deviceId);
        }

        if (userId == null)
        {
            userId = Guid.NewGuid().ToString();
            await _storageService.SetUserId(userId);
        }

        var consumer = new Consumer
        {
            AppKey = appId,
            DeviceId = deviceId,
            DeviceModel = _appInfoService.DeviceModel,
            UserId = userId,
            UserEmail = userEmail,
            OS = _appInfoService.OS,
            Country = _appInfoService.Country,
            Language = _appInfoService.Language,
        };
        
        return new RegisterEndpoint(consumer);
    }
}