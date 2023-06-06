using System;
namespace Kava.Logging
{
	public class LogManager
	{
		readonly ILogService logger;
		readonly INetworkLogService networkLogService;

		const int DEFAULT_LOG_SIZE = 1024; //kilobytes
		const int DEFAULT_NETWORK_LOG_INTERVAL = 20000;

		private int maxLogSizeKB = DEFAULT_LOG_SIZE;
		private bool sendOnStart = true;
		private bool sendOnClear = true;
		private int networkLogIntervalMS = DEFAULT_NETWORK_LOG_INTERVAL;

		public LogManager(ILogService logger, INetworkLogService networkLogService)
		{
			this.logger = logger;
			this.networkLogService = networkLogService;
		}

		public void Log(string message, LogLevel level = LogLevel.INFO, string tag = ILogService.DEFAULT_TAG)
		{
			logger.Log(message, level, tag);
		}
	}
}

