using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class StartSessionEndpoint : BaseEndpoint
{
    public StartSessionEndpoint()
    {
        Url = "/session/start";
        Method = HttpMethodEnum.Post;
        Payload = new
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }
}