using AppAmbit.Models.Analytics;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class SendAnalyticsEndpoint : BaseEndpoint
{
    public SendAnalyticsEndpoint(Models.Analytics.AnalyticsReport analyticsReport)
    {
        Url = "/analytics";
        Method = HttpMethodEnum.Post;
        Payload = new
        {
            analytics = analyticsReport
        };
    }
}