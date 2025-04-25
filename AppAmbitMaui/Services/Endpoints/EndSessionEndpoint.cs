using AppAmbit.Models.Analytics;
using AppAmbit.Models.App;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class EndSessionEndpoint : BaseEndpoint
{
    public EndSessionEndpoint(string sessionId)
    {
        Url = "/session/end";
        Method = HttpMethodEnum.Post;
        Payload = new EndSession()
        {
            Id = sessionId,
            Timestamp = DateTime.UtcNow
        };
    }
    
    public EndSessionEndpoint(EndSession endSession)
    {
        Url = "/session/end";
        Method = HttpMethodEnum.Post;
        Payload = endSession;
    }
}