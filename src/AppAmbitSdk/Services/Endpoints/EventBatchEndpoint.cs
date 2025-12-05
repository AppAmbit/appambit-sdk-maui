using AppAmbitSdkCore.Models.Analytics;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class EventBatchEndpoint: BaseEndpoint, IEndpoint
{
    public EventBatchEndpoint(List<EventEntity> eventBatch)
    {
        Url = "/events/batch";
        Method = HttpMethodEnum.Post;
        Payload = new EventBatchPayload
        {
            Events = eventBatch
        };
    }
}