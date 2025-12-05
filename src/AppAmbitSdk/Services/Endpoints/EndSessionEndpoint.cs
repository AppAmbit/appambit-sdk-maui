using AppAmbitSdkCore.Models.Analytics;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class EndSessionEndpoint : BaseEndpoint
{
    public EndSessionEndpoint(string sessionId)
    {
        Url = "/session/end";
        Method = HttpMethodEnum.Post;
        Payload = new SessionData()
        {
            SessionId = sessionId,
            Timestamp = DateTime.UtcNow
        };
    }
    
    public EndSessionEndpoint(SessionData endSession)
    {
        Url = "/session/end";
        Method = HttpMethodEnum.Post;
        Payload = endSession;
    }
}