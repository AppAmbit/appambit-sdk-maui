using System;
using Kava.Helpers;
using Microsoft.Extensions.Logging;

namespace Kava.Logging
{
	public class LogManager
	{
		readonly ILogService logger;
		readonly INetworkLogService networkLogService;

		const long DEFAULT_LOG_SIZE = 1024 * 1000; //1 megabyte
		const int DEFAULT_NETWORK_LOG_INTERVAL = 20000;

		private long maxLogSizeKB = DEFAULT_LOG_SIZE;
		private bool sendOnStart = true;
		private bool sendOnClear = true;
		private int networkLogIntervalMS = DEFAULT_NETWORK_LOG_INTERVAL;


		public long LogSizeKB { get => maxLogSizeKB; set => maxLogSizeKB = value * 1000; }

		public  LogLevel LoggingLevel { set => logger.ConsoleLogLevel = value; }

		public LogManager(ILogService logger, INetworkLogService networkLogService)
		{
			this.logger = logger;
			this.networkLogService = networkLogService;
		}

		public void Log(string message, LogLevel level = LogLevel.Information, string tag = ILogService.DEFAULT_TAG)
		{
			logger.Log(message, level, tag);
			if (ShouldClearLogs())
				Task.Run(async () => await StoreAndClearLogs());
		}

		public void ClearLogs()
		{
			logger.ClearLogs();
		}

		public async Task<LogEntry[]> GetLogs() => await logger.GetLogEntries();

		public async Task StoreAndClearLogs()
		{
			var entries = await logger.GetLogEntries();
			await networkLogService.UploadLogEntries(entries);
			await logger.ClearLogs();
		}

		private bool ShouldClearLogs() => FileHelper.GetFileSize(logger.GetLogFilePath()) > maxLogSizeKB;


	}
}

