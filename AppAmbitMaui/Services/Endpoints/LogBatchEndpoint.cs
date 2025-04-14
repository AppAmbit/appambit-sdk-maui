using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppAmbit.Services.Endpoints;

internal class LogBatchEndpoint: BaseEndpoint, IEndpoint
{
    public LogBatchEndpoint(List<LogEntity> logEntityList)
    {
        Url = "/log/batch";
        Method = HttpMethodEnum.Post;
        Payload = logEntityList;
    }
}