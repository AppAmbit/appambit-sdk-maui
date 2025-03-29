using System.Diagnostics;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit;

public static class Crashes
{
    public static async Task LogError(Exception? ex, Dictionary<string, string> properties = null)
    {
        var tempDict = new Dictionary<string, string>();
        if (properties != null)
        {
            foreach (var item in properties.TakeWhile(item => tempDict.Count <= 80))
            {
                tempDict.Add(item.Key, Truncate(item.Value, 125));
            }
        }
        
        await LogEvent( ex?.StackTrace, LogType.Error, ex, JsonConvert.SerializeObject(tempDict));
    }
    
    public static async Task LogError(string message)
    {
        await LogEvent(message, LogType.Error);
    }

    public static async Task GenerateTestCrash()
    {
        await LogEvent( "This is a test crash", LogType.Crash);
    }
    
    private static async Task LogEvent(string? message, LogType logType, Exception? exception = null, string properties = null)
    {
        var stackTrace = exception?.StackTrace;
        stackTrace = (String.IsNullOrEmpty(stackTrace)) ? AppConstants.NoStackTraceAvailable : stackTrace;
        var log = new Log
        {
            AppVersion = $"{AppInfo.VersionString} ({AppInfo.BuildString})",
            ClassFQN = exception?.TargetSite?.DeclaringType?.FullName ?? AppConstants.UnknownClass,
            FileName = exception?.GetFileNameFromStackTrace() ?? AppConstants.UnknownFileName,
            LineNumber = exception?.GetLineNumberFromStackTrace() ?? 0,
            Message = "" + message,
            StackTrace = stackTrace,
            Context = new Dictionary<string, object>(),
            Type = logType
        };
        var storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
        await storageService?.LogEventAsync(log);
            
    }
    
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}