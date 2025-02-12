using Shared.Models.App;
using Shared.Models.Endpoints.Base;

namespace Shared.Models.Endpoints;

public class EndSessionEndpoint : BaseEndpoint
{
    public EndSessionEndpoint(Session session)
    {
        Url = "/session/end";
        Method = HttpMethodEnum.Post;
        Payload = new
        {
            session
        };
    }
}