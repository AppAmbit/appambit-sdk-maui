using Shared.Models.Endpoints.Base;

namespace Shared.Models.Endpoints;

public class StartSessionEndpoint : BaseEndpoint
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