using AppAmbit.Models;
using AppAmbit.Models.App;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class RegisterEndpoint : BaseEndpoint, IEndpoint
{
    public RegisterEndpoint(Consumer consumer)
    {
        Url = "/consumer";
        Method = HttpMethodEnum.Post;
        Payload = consumer;
    }
}