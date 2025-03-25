using Shared.Models.App;
using Shared.Models.Endpoints.Base;

namespace Shared.Models.Endpoints;

public class RegisterEndpoint : BaseEndpoint, IEndpoint
{
    public RegisterEndpoint(Consumer consumer)
    {
        Url = "/app-consumer/register";
        Method = HttpMethodEnum.Post;
        Payload = new
        {
            consumer
        };
    }
}