using AppAmbit.Models.Analytics;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;
using Shared.Utils;

namespace AppAmbit.Services.Endpoints;

internal class EndSessionEndpoint : BaseEndpoint
{
    public EndSessionEndpoint(string sessionId)
    {
        Url = "/session/end";
        Method = HttpMethodEnum.Post;
        Payload = new SessionData()
        {
            SessionId = sessionId,
            Timestamp = DateUtils.GetUtcNow
        };
    }
    
    public EndSessionEndpoint(SessionData endSession)
    {
        Url = "/session/end";
        Method = HttpMethodEnum.Post;
        Payload = endSession;
    }
}