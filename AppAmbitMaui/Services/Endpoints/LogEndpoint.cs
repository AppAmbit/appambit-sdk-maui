using AppAmbit.Models;
using AppAmbit.Models.App;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class LogEndpoint : BaseEndpoint, IEndpoint
{
    public LogEndpoint(Log log)
    {
        Url = "/log";
        Method = HttpMethodEnum.Post;
        Payload = log;
    }
}