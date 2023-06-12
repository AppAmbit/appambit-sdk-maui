using System;
using Microsoft.Extensions.Logging;

namespace Kava.Logging
{ 
	public interface ILogService
	{
		internal const string DEFAULT_TAG = "DEFAULT";

		Task Log(string message, LogLevel level = LogLevel.Information, string tag = DEFAULT_TAG);

		Task<LogEntry> LogAsync(string message, LogLevel level = LogLevel.Information, string tag = DEFAULT_TAG);

		Task<LogEntry[]> GetLogEntries();

		Task<bool> ClearLogs();

		DateTime getLastLogTime();

		string GetLogFilePath();

		LogLevel ConsoleLogLevel { set; }

    }
}

