using Shared.Models.Analytics;
using Shared.Models.Endpoints.Base;

namespace Shared.Models.Endpoints;

public class SendAnalyticsEndpoint : BaseEndpoint
{
    public SendAnalyticsEndpoint(AnalyticsReport analyticsReport)
    {
        Url = "/analytics";
        Method = HttpMethodEnum.Post;
        Payload = new
        { 
            analyticsReport
        };
    }
}