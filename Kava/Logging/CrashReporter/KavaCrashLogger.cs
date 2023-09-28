using System.Globalization;
using Kava.Helpers;
using Microsoft.Extensions.Logging;

namespace Kava.Logging.CrashReporter;

public class KavaCrashLogger
{
    	readonly static string CRASH_FILE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
	    const string LOG_DIRECTORY = "KavaTemp";
	    const string LOG_FILE = "Log.txt";
	    const string CRASH_DIRECTORY = "KavaTempCrash";
	    
	    private readonly Guid _crashId = Guid.NewGuid();
	    private string _crashFile;
	    private DeviceHelper _deviceHelper = new DeviceHelper();
    	const LogLevel _consoleLogLevel = LogLevel.Critical;
    
    	public KavaCrashLogger()
	    {
		    InitializeCrashFile();
	    }
    
    	public async Task<List<FileInfo>> GetCrashFiles()
	    {
		    await Task.Delay(1000);
		    return null;
	    }
    
    	public DateTime GetLastLogTime() => File.GetLastWriteTime(GetLogFilePath());
	    
	    public async Task<bool> ClearCrashLogs()
	    {
		    return await Task.Run(() => FileHelper.ClearLog(CRASH_FILE_PATH, CRASH_DIRECTORY));
	    }
    
    	public async Task EnterCrashLog(UnhandledExceptionEventArgs unhandledExceptionEventArgs, LogLevel level = LogLevel.Critical, string tag = "CRASH")
	    {
		    Task[] tasks = new Task[]
		    {
			    Task.Run(() =>
			    {
				    LogCrashToLoggerAsync($"Crash {_crashId.ToString()} Detected!", level, tag);
				    SaveCrashLog(unhandledExceptionEventArgs);
			    }),
		    };

		   Task.WaitAll(tasks);
	    }    
    
    	private void LogCrashToLoggerAsync(string message, LogLevel level = LogLevel.Information, string tag = "DEFAULT")
    	{
    		var entry = new LogEntry
    		{
    			Message = message,
    			LogLevel = level,
    			LogTag = tag,
    			CreatedAt = DateTime.Now
    		};
    
		    SaveLogToFile(entry);
    	}
	    
	    private void SaveLogToFile(LogEntry entry)
    	{
    		FileHelper.AddTextToFile(entry.Parse(), GetLogFilePath());
    	}
	    
	    private void SaveCrashLogToFile(String contents)
	    {
		    FileHelper.AddTextToFile(contents, GetCrashLogFilePath());
	    }

	    private void SaveCrashLog(UnhandledExceptionEventArgs unhandledExceptionEventArgs)
	    {
		    var exception = unhandledExceptionEventArgs.ExceptionObject as Exception;
		    var message = exception?.Message ?? "No message provided";
		    var stackTrace = exception?.StackTrace ?? "No stacktrace provided";
		    
		    SaveCrashLogToFile(message);
		    SaveCrashLogToFile(stackTrace);
	    }
	    
	    private void InitializeCrashFile()
	    {
		    _crashFile = $"Crash_{_deviceHelper.GetDeviceId()}_{_crashId.ToString()}.log";
		    FileHelper.CreateFileWithDirectory(CRASH_FILE_PATH, CRASH_DIRECTORY, _crashFile);
	    }
    
    	private string GetLogFilePath() => Path.Combine(CRASH_FILE_PATH, LOG_DIRECTORY, LOG_FILE);
	    
	    private string GetCrashLogFilePath() => Path.Combine(CRASH_FILE_PATH, CRASH_DIRECTORY, _crashFile);
}