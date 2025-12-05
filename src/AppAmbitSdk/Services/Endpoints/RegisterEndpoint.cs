using AppAmbitSdkCore.Models;
using AppAmbitSdkCore.Models.App;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class RegisterEndpoint : BaseEndpoint, IEndpoint
{
    public RegisterEndpoint(Consumer consumer)
    {
        Url = "/consumer";
        Method = HttpMethodEnum.Post;
        Payload = consumer;
    }
}