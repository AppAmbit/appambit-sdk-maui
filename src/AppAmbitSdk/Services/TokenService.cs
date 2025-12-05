using System.Diagnostics;
using AppAmbitSdkCore.Models.App;
using AppAmbitSdkCore.Services.Endpoints;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services;

internal class TokenService
{
    private static IStorageService? _storageService;

    public static void Initialize(IStorageService? storageService)
    {
        _storageService = storageService;
    }
    public static async Task<TokenEndpoint> CreateTokenendpoint()
    {
        try
        {
            if (_storageService == null)
            {
                throw new ArgumentNullException(nameof(_storageService));
            }

            var appKey = await _storageService.GetAppId();
            var consumerId = await _storageService.GetConsumerId();

            return new TokenEndpoint(new ConsumerToken()
            {
                appKey = appKey ?? "",
                 consumerId = consumerId ?? ""
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return new TokenEndpoint(new ConsumerToken());
        }
    }
}
