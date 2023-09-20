using Kava.Helpers;
using Microsoft.Extensions.Logging;

namespace Kava.Logging;

public class KavaLogger : ILogService
{

	readonly static string LOG_FILE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
	const string LOG_DIRECTORY = "KavaTemp";
	const string LOG_FILE = "Log.txt";

	private LogLevel _consoleLogLevel { get; set; } = LogLevel.Information;
	LogLevel ILogService.ConsoleLogLevel { set => _consoleLogLevel = value; }

	public KavaLogger()
	{
		FileHelper.CreateFileWithDirectory(LOG_FILE_PATH, LOG_DIRECTORY, LOG_FILE);
	}

	public async Task<LogEntry[]> GetLogEntries()
	{
		var entries = await FileHelper.GetFileContents(GetLogFilePath());

		if (entries == null)
			return new LogEntry[0];

		return entries.Select(entry => LogEntry.UnParse(entry)).OrderBy(entry => entry.CreatedAt).ToArray();
	}

	public DateTime getLastLogTime() => File.GetLastWriteTime(GetLogFilePath());

	public Task Log(string message, LogLevel level = LogLevel.Information, string tag = "DEFAULT") => Task.Run(async () =>
	{
		await LogAsync(message, level, tag);
	});    

	public async Task<LogEntry> LogAsync(string message, LogLevel level = LogLevel.Information, string tag = "DEFAULT")
	{
		var entry = new LogEntry
		{
			Message = message,
			LogLevel = level,
			LogTag = tag,
			CreatedAt = DateTime.Now
		};

		if (entry.LogLevel >= _consoleLogLevel)
			Console.WriteLine(entry.ToString());

		await SaveLogToFile(entry);
		return entry;
	}

	async Task SaveLogToFile(LogEntry entry)
	{
		await Task.Run(() =>
		{
			FileHelper.AddTextToFile(entry.Parse(), GetLogFilePath());
		});
	}

	public async Task<bool> ClearLogs()
	{
		return await Task.Run(() =>
		{
			return FileHelper.ClearLog(LOG_FILE_PATH, LOG_DIRECTORY, LOG_FILE);
		});
	}

	public string GetLogFilePath() => Path.Combine(LOG_FILE_PATH, LOG_DIRECTORY, LOG_FILE);
}