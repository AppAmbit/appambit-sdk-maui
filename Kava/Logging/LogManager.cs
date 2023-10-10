using Kava.Helpers;
using Microsoft.Extensions.Logging;

namespace Kava.Logging;

public class LogManager
{
	readonly ILogService _logger;
	readonly INetworkLogService _networkLogService;

	public const long DefaultLogSizeMb = 1024 * 1000; //1 megabyte
	const int DefaultNetworkLogInterval = 20000;

	private long _maxLogSizeKb = DefaultLogSizeMb;
	private bool _sendOnStart = true;
	private bool _sendOnClear = true;
	private int _networkLogIntervalMs = DefaultNetworkLogInterval;


	public long LogSizeKb { get => _maxLogSizeKb; set => _maxLogSizeKb = value * 1000; }
	
	public LogManager(ILogService logger, INetworkLogService networkLogService)
	{
		this._logger = logger;
		this._networkLogService = networkLogService;
	}

	public void Log(string message, LogLevel level = LogLevel.Information, string tag = ILogService.DEFAULT_TAG)
	{
		_logger.Log(message, level, tag);
		if (ShouldClearLogs())
			Task.Run(async () => await StoreAndClearLogs());
	}

	public void ClearLogs()
	{
		_logger.ClearLogs();
	}
	
	public async Task StoreAndClearLogs()
	{
		await _logger.ClearLogs();
	}
	
	// gets the filepath from the logger
	// returns a file from the filepath
	public async Task<string[]> GetLogs()
	{
		return await Task.Run(() => FileHelper.GetFileContents(LogHelper.GetLogFilePath()));
	}
	

	// This needs to be refactored as this doesn't wor the same on both impleentations
	private bool ShouldClearLogs() => _logger.ShouldClearLogs();


}