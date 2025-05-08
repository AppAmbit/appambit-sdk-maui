using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;
using Shared.Utils;

namespace AppAmbit.Services.Endpoints;

internal class StartSessionEndpoint : BaseEndpoint
{
    public StartSessionEndpoint()
    {
        Url = "/session/start";
        Method = HttpMethodEnum.Post;
        Payload = new
        {
            timestamp = DateUtils.GetUtcNowFormatted
        };
    }
}