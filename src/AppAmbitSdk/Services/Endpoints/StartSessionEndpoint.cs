using AppAmbitSdkCore.Models.Analytics;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class StartSessionEndpoint : BaseEndpoint
{
    public StartSessionEndpoint(DateTime utcNow)
    {
        Url = "/session/start";
        Method = HttpMethodEnum.Post;
        Payload = new SessionData
        {
            Timestamp = utcNow
        };
    }
}