using Microsoft.Extensions.Logging;

namespace Kava.Logging;

public interface ILogService
{
	internal const string DEFAULT_TAG = "DEFAULT";

	Task Log(string message, LogLevel level = LogLevel.Information, string tag = DEFAULT_TAG);

	Task<bool> LogAsync(string message, LogLevel level = LogLevel.Information, string tag = DEFAULT_TAG);
	
	Task Log(LogEntry entry); 
	
	Task<bool> LogAsync(LogEntry entry);
	
	Task<bool> ClearLogs();

	public bool ShouldClearLogs();
}