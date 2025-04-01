using System.Diagnostics;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit;

public static class Crashes
{
    public static void Initialize()
    {
        RegisterUnhandledExceptions();
    }

    private static void RegisterUnhandledExceptions()
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= UnobservedTaskException;
        TaskScheduler.UnobservedTaskException += UnobservedTaskException;
    }
    
    public static async Task LogError(Exception? ex, Dictionary<string, object> properties = null)
    {
        await LogEvent( ex?.Message, LogType.Error, ex, properties);
    }
    
    public static async Task LogError(string message, Dictionary<string, object> properties = null)
    {
        await LogEvent(message, LogType.Error,null,properties);
    }

    public static async Task GenerateTestCrash()
    {
        throw new NullReferenceException();
    }

    private static async Task LogEvent(string? message, LogType logType, Exception? exception = null,
        Dictionary<string,object> properties = null)
    {
        await Logging.LogEvent(message, LogType.Error,exception, properties);
    }
    
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
    
    private static void UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {   
        var exception = e?.Exception;
        var message = exception?.Message;
        LogEvent(message, LogType.Crash, exception);
        Core.OnSleep();
    }
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        var exception = unhandledExceptionEventArgs.ExceptionObject as Exception;
        var message = exception?.Message;
        LogEvent(message, LogType.Crash, exception);
        Core.OnSleep();
    }
}