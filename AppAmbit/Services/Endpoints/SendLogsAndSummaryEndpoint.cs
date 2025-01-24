using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints.Base;
using AppAmbit.Services.Interfaces;

namespace AppAmbit.Services.Endpoints;

internal class SendLogsAndSummaryEndpoint : BaseEndpoint
{
    public SendLogsAndSummaryEndpoint(ByteArrayContent logFile, LogSummary logSummary)
    {
        Url = "/app/log";
        Method = HttpMethodEnum.Post;
        Payload = new
        {
            logFile,
            logSummary
        };
    }
}