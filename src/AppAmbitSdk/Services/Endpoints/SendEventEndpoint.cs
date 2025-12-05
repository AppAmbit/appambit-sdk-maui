using AppAmbitSdkCore.Models.Analytics;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class SendEventEndpoint : BaseEndpoint
{
    public SendEventEndpoint(Models.Analytics.Event analyticsReport)
    {
        Url = "/events";
        Method = HttpMethodEnum.Post;
        Payload = analyticsReport;
    }
}