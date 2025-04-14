using System.Diagnostics;
using System.Runtime.CompilerServices;
using AppAmbit.Models.App;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit;

public static class Crashes
{
    private static IStorageService? _storageService;
    private static string _deviceId;
    private static bool _crashedInLastSession = false;
    internal static void Initialize(IAPIService? apiService,IStorageService? storageService, string deviceId)
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= UnobservedTaskException;
        TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        _storageService = storageService;
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
    
    
    public static async void LoadCrashFileIfExists()
    {
        var crashFile = Path.Combine(FileSystem.AppDataDirectory, "last_crash.json");

        if (!File.Exists(crashFile))
        {
            _crashedInLastSession = false;
            return;
        }

        var json = await File.ReadAllTextAsync(crashFile);
        File.Delete(crashFile);
        var exceptionInfo = JsonConvert.DeserializeObject<ExceptionInfo>(json);
        await LogCrash(exceptionInfo);
        _crashedInLastSession = true;
    }
    
    public static async Task<bool> CrashedInLastSession()
    {
        return _crashedInLastSession;
    }
    
    private static async Task LogCrash(ExceptionInfo? exception = null)
    {
        var message = exception?.Message;
        await Logging.LogEvent(message, LogType.Crash,exception);
    }
    
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
    
    private static async void UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var exception = ExceptionInfo.FromException(e?.Exception);
        await LogCrash(exception);
    }
    
    private static async void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        if (unhandledExceptionEventArgs.ExceptionObject is not Exception ex)
            return;

        var info = ExceptionInfo.FromException(ex,_deviceId);
        var json = JsonConvert.SerializeObject(info, Formatting.Indented);
        Debug.WriteLine($"AppDataDirectory:{FileSystem.AppDataDirectory}");
        var crashFile = Path.Combine(FileSystem.AppDataDirectory, "last_crash.json");
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
}