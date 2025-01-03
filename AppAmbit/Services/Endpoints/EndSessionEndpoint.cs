using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class EndSessionEndpoint : BaseEndpoint
{
    public EndSessionEndpoint(string? sessionId)
    {
        Url = "/session/end";
        Method = HttpMethodEnum.Post;
        Payload = new
        {
            DateTime.Now,
            sessionId
        };
    }
}