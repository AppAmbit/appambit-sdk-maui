using AppAmbit.Models.Analytics;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class EventBatchEndpoint: BaseEndpoint, IEndpoint
{
    public EventBatchEndpoint(List<EventEntity> eventBatch)
    {
        Url = "/events/batch";
        Method = HttpMethodEnum.Post;
        Payload = eventBatch;
    }
}