using AppAmbitSdkCore.Models.Breadcrumbs;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class BreadcrumbsBatchEndpoint : BaseEndpoint
{
    public BreadcrumbsBatchEndpoint(List<BreadcrumbData> batch)
    {
        Url = "/breadcrumbs/batch";
        Method = HttpMethodEnum.Post;
        Payload = new BreadcrumbsPayload
        {
            Breadcrumbs = batch
        };
    }
}
