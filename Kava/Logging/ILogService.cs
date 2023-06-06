using System;
namespace Kava.Logging
{ 
	public interface ILogService
	{
		internal const string DEFAULT_TAG = "DEFAULT";

		Task Log(string message, LogLevel level = LogLevel.INFO, string tag = DEFAULT_TAG);

		Task<LogEntry> LogAsync(string message, LogLevel level = LogLevel.INFO, string tag = DEFAULT_TAG);

		Task<LogEntry[]> GetLogEntries();

		DateTime getLastLogTime();
	}
}

