using System;
using AppAmbit.Models.App;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class TokenEndpoint : BaseEndpoint
{
    public TokenEndpoint(ConsumerToken consumerToken)
    {
        Url = "/consumer/token";
        Method = HttpMethodEnum.Get;
        Payload = consumerToken;
    }
}
