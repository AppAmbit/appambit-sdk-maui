using Shared.Models.Endpoints.Base;
using Shared.Models.Logs;

namespace Shared.Models.Endpoints;

public class SendLogsAndSummaryEndpoint : BaseEndpoint
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