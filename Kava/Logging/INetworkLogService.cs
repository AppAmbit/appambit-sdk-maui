namespace Kava.Logging;

public interface INetworkLogService
{
	Task UploadLogEntries(LogEntry[] entries);
}