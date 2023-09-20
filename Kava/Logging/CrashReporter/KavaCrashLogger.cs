using Kava.Helpers;
using Microsoft.Extensions.Logging;

namespace Kava.Logging.CrashReporter;

public class KavaCrashLogger
{
    	readonly static string CRASH_FILE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
	    const string LOG_DIRECTORY = "KavaTemp";
	    const string LOG_FILE = "Log.txt";
	    const string CRASH_DIRECTORY = "KavaTempCrash";
	    private string CRASH_FILE;
    
    	const LogLevel _consoleLogLevel = LogLevel.Critical;
    
    	public KavaCrashLogger()
    	{
    	}
    
    	public async Task<List<FileInfo>> GetCrashFiles()
	    {
		    await Task.Delay(1000);
		    return null;
	    }
    
    	public DateTime getLastLogTime() => File.GetLastWriteTime(GetLogFilePath());
    
    	public async Task EnterCrashLog(UnhandledExceptionEventArgs unhandledExceptionEventArgs, LogLevel level = LogLevel.Critical, string tag = "CRASH")
	    {
		    await LogCrashToLoggerAsync("Crash Detected!", level, tag);
		    CRASH_FILE = $"{unhandledExceptionEventArgs.ExceptionObject.GetHashCode()}.log";
		    FileHelper.CreateFileWithDirectory(CRASH_FILE_PATH, CRASH_DIRECTORY, CRASH_FILE);
		    await SaveCrashLog(unhandledExceptionEventArgs);
	    }    
    
    	private async Task LogCrashToLoggerAsync(string message, LogLevel level = LogLevel.Information, string tag = "DEFAULT")
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
    	}
    
    	async Task SaveLogToFile(LogEntry entry)
    	{
    		await Task.Run(() =>
    		{
    			FileHelper.AddTextToFile(entry.Parse(), GetLogFilePath());
    		});
    	}
	    
	    async Task SaveCrashLogToFile(String contents)
	    {
		    await Task.Run(() =>
		    {
			    FileHelper.AddTextToFile(contents, GetCrashLogFilePath());
		    });
	    }

	    async Task SaveCrashLog(UnhandledExceptionEventArgs unhandledExceptionEventArgs)
	    {
		    String contents = unhandledExceptionEventArgs.ToString();
		    await SaveCrashLogToFile(contents);
	    }

	    public async Task<bool> ClearCrashLogs()
    	{
    		return await Task.Run(() =>
    		{
    			return FileHelper.ClearLog(CRASH_FILE_PATH, CRASH_DIRECTORY);
    		});
    	}
    
    	public string GetLogFilePath() => Path.Combine(CRASH_FILE_PATH, LOG_DIRECTORY, LOG_FILE);
	    
	    public string GetCrashLogFilePath() => Path.Combine(CRASH_FILE_PATH, CRASH_DIRECTORY, CRASH_FILE);
}