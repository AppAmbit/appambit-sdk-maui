using AppAmbitSdkCore.Models.Logs;
using AppAmbitSdkCore.Services.Endpoints.Base;
using AppAmbitSdkCore.Services.Interfaces;

namespace AppAmbitSdkCore.Services.Endpoints;

internal class LogBatchEndpoint: BaseEndpoint, IEndpoint
{
    public LogBatchEndpoint(LogBatch logBatch)
    {
        Url = "/log/batch";
        Method = HttpMethodEnum.Post;
        Payload = logBatch;
    }
}