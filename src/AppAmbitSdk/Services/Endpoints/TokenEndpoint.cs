using System;
using AppAmbitSdkCore.Models.App;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class TokenEndpoint : BaseEndpoint
{
    public TokenEndpoint(ConsumerToken consumerToken)
    {
        Url = "/consumer/token";
        Method = HttpMethodEnum.Get;
        Payload = consumerToken;
    }
}
