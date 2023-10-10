using Kava.Helpers;
using Microsoft.Extensions.Logging;

namespace Kava.Logging;

public class KavaLogger : ILogService
{
	const long DEFAULT_LOG_SIZE = 1024 * 1000; //1 megabyte
	const int DEFAULT_NETWORK_LOG_INTERVAL = 20000;

	private long maxLogSizeKB = DEFAULT_LOG_SIZE;
	
	public KavaLogger()
	{
		FileHelper.CreateFileWithDirectory(LogHelper.LogFilePath, LogHelper.LogDirectory, LogHelper.LogFileName);
	}
	
	public Task Log(string message, LogLevel level = LogLevel.Information, string tag = "DEFAULT") => Task.Run(async () =>
	{
		await LogAsync(message, level, tag);
	}); 

	public async Task<bool> LogAsync(LogEntry entry)
	{
		await SaveLogToFile(entry);
		if (ShouldClearLogs())
			await ClearLogs();	
		return true;
	}

	public async Task<bool> LogAsync(string message, LogLevel level = LogLevel.Information, string tag = "DEFAULT")
	{
		var entry = new LogEntry
		{
			Message = message,
			LogLevel = level,
			LogTag = tag,
			CreatedAt = DateTime.Now
		};

		if (entry.LogLevel >= LogLevel.None)
			Console.WriteLine(entry.ToString());

		return await LogAsync(entry);
	}

	public Task Log(LogEntry entry) => Task.Run(async () =>
	{
		await LogAsync(entry);
	});

	async Task SaveLogToFile(LogEntry entry)
	{
		await Task.Run(() =>
		{
			FileHelper.AddTextToFile(entry.Parse(), LogHelper.GetLogFilePath());
		});
	}

	public async Task<bool> ClearLogs()
	{
		return await Task.Run(() =>
		{
			return FileHelper.ClearLog(LogHelper.LogFilePath, LogHelper.LogDirectory, LogHelper.LogFileName);
		});
	}
	
	public bool ShouldClearLogs() => new FileInfo(LogHelper.GetLogFilePath()).Length > maxLogSizeKB;
}