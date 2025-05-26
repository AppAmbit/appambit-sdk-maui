using AppAmbit.Models.Analytics;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;
using Shared.Utils;

namespace AppAmbit.Services.Endpoints;

internal class StartSessionEndpoint : BaseEndpoint
{
    public StartSessionEndpoint(DateTime utcNow)
    {
        Url = "/session/start";
        Method = HttpMethodEnum.Post;
        Payload = new StartSession
        {
            Timestamp = utcNow
        };
    }
}