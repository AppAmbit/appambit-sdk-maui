using AppAmbitSdkCore.Models;
using AppAmbitSdkCore.Models.App;
using AppAmbitSdkCore.Models.Logs;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class LogEndpoint : BaseEndpoint, IEndpoint
{
    public LogEndpoint(Log log)
    {
        Url = "/log";
        Method = HttpMethodEnum.Post;
        Payload = log;
    }
}