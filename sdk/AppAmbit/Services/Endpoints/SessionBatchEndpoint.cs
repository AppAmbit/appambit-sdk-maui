
using AppAmbit.Models.Analytics;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class SessionBatchEndpoint : BaseEndpoint, IEndpoint
{
    public SessionBatchEndpoint(SessionsPayload sessionBatch)
    {
        Url = "/session/batch";
        Method = HttpMethodEnum.Post;
        Payload = sessionBatch;
    }
}
