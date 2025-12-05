
using AppAmbitSdkCore.Models.Analytics;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class SessionBatchEndpoint : BaseEndpoint, IEndpoint
{
    public SessionBatchEndpoint(SessionsPayload sessionBatch)
    {
        Url = "/session/batch";
        Method = HttpMethodEnum.Post;
        Payload = sessionBatch;
    }
}
