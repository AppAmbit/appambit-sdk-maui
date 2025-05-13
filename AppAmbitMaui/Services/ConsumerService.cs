using AppAmbit.Models.App;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppAmbit.Services;

internal class ConsumerService
{
    private static IAPIService? _apiService;
    private static IStorageService? _storageService;
    private static IAppInfoService? _appInfoService;
    private static bool _TokenExpired = true;

    public static void Initialize(IAPIService? apiService, IStorageService? storageService, IAppInfoService? appInfoService)
    {
        _apiService = apiService;
        _storageService = storageService;
        _appInfoService = appInfoService;
    }

    public static async Task<bool> CreateToken(string appKey = "")
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
        var registerEndpoint = new RegisterEndpoint(consumer);
        var remoteToken = await _apiService?.ExecuteRequest<TokenResponse>(registerEndpoint);
        if (remoteToken == null)
        {
            _apiService.SetToken("");
            return false;
        }

        _apiService.SetToken(remoteToken?.Token);
        return true;
    }
}