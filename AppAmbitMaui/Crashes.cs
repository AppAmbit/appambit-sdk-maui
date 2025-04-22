using System.Diagnostics;
using System.Runtime.CompilerServices;
using AppAmbit.Models.App;
using AppAmbit.Models.Logs;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit;

public static class Crashes
{
    private static IStorageService? _storageService;
    private static IAPIService? _apiService;
    private static string _deviceId;
    private static bool _didCrashInLastSession = false;
    internal static void Initialize(IAPIService? apiService,IStorageService? storageService, string deviceId)
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= UnobservedTaskException;
        TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        
        _storageService = storageService;
        _apiService = apiService;
        _deviceId = deviceId;
        Logging.Initialize(apiService,storageService);
    }
    
    public static async Task LogError(Exception? exception, Dictionary<string,string> properties = null, string? classFqn = null,[CallerFilePath] string? fileName = null,[CallerLineNumber] int lineNumber = 0)
    {
        classFqn = classFqn ?? await GetCallerClassAsync(); 
        await Logging.LogEvent("", LogType.Error,exception, properties,classFqn,fileName,lineNumber);
    }
    
    public static async Task LogError(string message, Dictionary<string,string> properties = null, string? classFqn = null, Exception? exception = null,[CallerFilePath] string? fileName = null,[CallerLineNumber] int? lineNumber = null)
    {
        classFqn = classFqn ?? await GetCallerClassAsync();
        await Logging.LogEvent(message, LogType.Error,exception, properties,classFqn,fileName,lineNumber);
    }

    public static async Task GenerateTestCrash()
    {
        throw new NullReferenceException();
    }
    
    internal static async void LoadCrashFileIfExists()
    {
        var crashFile = GetCrashFilePath();

        if (!CrashFileExists(crashFile))
        {
            SetCrashFlag(false);
            return;
        }

        SetCrashFlag(true);

        var exceptionInfo = await ReadAndDeleteCrashFileAsync(crashFile);

        if (exceptionInfo is not null)
        {
            await LogCrash(exceptionInfo);
        }
    }
    
    public static async Task<bool> DidCrashInLastSession()
    {
        return _didCrashInLastSession;
    }
    
    private static async Task LogCrash(ExceptionInfo? exception = null)
    {
        var message = exception?.Message;
        await Logging.LogEvent(message, LogType.Crash,exception);
    }
    
    private static async Task LogError(ExceptionInfo? exception = null)
    {
        var message = exception?.Message;
        await Logging.LogEvent(message, LogType.Error,exception);
    }
    
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
    
    private static async void UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var exception = ExceptionInfo.FromException(e?.Exception);
        await LogError(exception);
    }
    
    private static async void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        if (unhandledExceptionEventArgs.ExceptionObject is not Exception ex)
            return;

        var info = ExceptionInfo.FromException(ex,_deviceId);
        var json = JsonConvert.SerializeObject(info, Formatting.Indented);
        
        SaveCrashToFile(json);
    }
    private static void SaveCrashToFile(string json)
    {
        var crashFile = GetCrashFilePath();
        Debug.WriteLine($"AppDataDirectory: {FileSystem.AppDataDirectory}");
        File.WriteAllText(crashFile, json);
    }
    
    private static async Task<string?> GetCallerClassAsync()
    {
        var listSystemNames = new List<string>()
        {
            $"System.",
            $"AppAmbit.",
            $"Foundation.",
            $"UIKit.",
        };
        await Task.Yield();
        var stackTrace = new StackTrace();
        var classFqn = (string?)null;
        foreach (var frame in stackTrace.GetFrames())
        {
            var method = frame?.GetMethod();
            var fullName =  method?.DeclaringType?.FullName ?? "";
            if(!ContainsFromList(fullName,listSystemNames))
            {
                classFqn = fullName;
            }
        }
        return classFqn;
    }

    private static bool ContainsFromList(string word, List<string> list)
    {
        foreach (var s in list)
        {
            if(word.Contains(s))
                return true;
        }
        return false;
    }
    
    public static async Task SendBatchLogs()
    {
        Debug.WriteLine("SendBatchLogs");
        var logEntityList = await _storageService.GetOldest100LogsAsync();
        if (logEntityList?.Count == 0)
        {
            Debug.WriteLine("No logs to send");
            return;
        }

        Debug.WriteLine("Sending logs in batch");
        var logBatch = new LogBatch() { Logs = logEntityList };
        var endpoint = new LogBatchEndpoint(logBatch);
        var logResponse = await _apiService?.ExecuteRequest<Response>(endpoint);
        await _storageService.DeleteLogList(logEntityList);
        Debug.WriteLine("Logs batch sent");
    }
    
    private static string GetCrashFilePath()
    {
        return Path.Combine(FileSystem.AppDataDirectory, "last_crash.json");
    }
    
    private static bool CrashFileExists(string path)
    {
        return File.Exists(path);
    }
    
    private static void SetCrashFlag(bool didCrash)
    {
        _didCrashInLastSession = didCrash;
    }
    
    private static async Task<ExceptionInfo?> ReadAndDeleteCrashFileAsync(string path)
    {
        try
        {
            var json = await File.ReadAllTextAsync(path);
            File.Delete(path);
            return JsonConvert.DeserializeObject<ExceptionInfo>(json);
        }
        catch
        {
            return null;
        }
    }
}