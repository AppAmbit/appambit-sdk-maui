using AppAmbit.Models.Breadcrumbs;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

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
