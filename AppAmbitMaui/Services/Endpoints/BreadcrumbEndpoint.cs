
using AppAmbit.Models.Breadcrums;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class BreadcrumbEndpoint : BaseEndpoint
{
    public BreadcrumbEndpoint(BreadcrumbsEntity breadcrumb)
    {
        Url = "/breadcrumbs";
        Method = HttpMethodEnum.Post;
        Payload = breadcrumb; 
    }
}
