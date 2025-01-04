using AppAmbit.Models.App;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class EndSessionEndpoint : BaseEndpoint
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