using AppAmbit.Models.Analytics;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class SendEventEndpoint : BaseEndpoint
{
    public SendEventEndpoint(Event analyticsReport)
    {
        Url = "/events";
        Method = HttpMethodEnum.Post;
        Payload = analyticsReport;
    }
}