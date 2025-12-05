
using AppAmbitSdkCore.Models.Breadcrumbs;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class BreadcrumbEndpoint : BaseEndpoint
{
    public BreadcrumbEndpoint(BreadcrumbsEntity breadcrumb)
    {
        Url = "/breadcrumbs";
        Method = HttpMethodEnum.Post;
        Payload = breadcrumb; 
    }
}
