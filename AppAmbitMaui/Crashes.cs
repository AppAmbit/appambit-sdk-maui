using System.Diagnostics;
using System.Text;
using System.Runtime.CompilerServices;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;
    
#if ANDROID
#elif IOS
using UIKit;
#endif

namespace AppAmbit;

public static class Crashes
{
    internal static void Initialize(IAPIService? apiService,IStorageService? storageService)
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= UnobservedTaskException;
        TaskScheduler.UnobservedTaskException += UnobservedTaskException;
        Logging.Initialize(apiService,storageService);
    }
    
    public static async Task LogError(Exception? exception, Dictionary<string, object> properties = null, string? classFqn = null,[CallerFilePath] string? fileName = null,[CallerLineNumber] int lineNumber = 0)
    {
        classFqn = classFqn ?? await GetCallerClassAsync(); 
        await Logging.LogEvent("", LogType.Error,exception, properties,classFqn,fileName,lineNumber);
    }
    
    public static async Task LogError(string message, Dictionary<string, object> properties = null, string? classFqn = null, Exception? exception = null,[CallerFilePath] string? fileName = null,[CallerLineNumber] int? lineNumber = null)
    {
        classFqn = classFqn ?? await GetCallerClassAsync();
        await Logging.LogEvent(message, LogType.Error,exception, properties,classFqn,fileName,lineNumber);
    }

    public static async Task GenerateTestCrash()
    {
        throw new NullReferenceException();
    }
    
    private static async Task LogCrash(Exception? exception = null)
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
        var exception = e?.Exception;
        await LogCrash(exception);
    }
    
    private static async void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        var exception = unhandledExceptionEventArgs.ExceptionObject as Exception;
        await LogCrash(exception);
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