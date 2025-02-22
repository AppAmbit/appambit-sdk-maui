using AppAmbit.Models.Logs;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit;

public static class Crashes
{
    public static async Task TrackError(Exception? ex, Dictionary<string, string> properties = null)
    {
        var tempDict = new Dictionary<string, string>();
        if (properties != null)
        {
            foreach (var item in properties.TakeWhile(item => tempDict.Count <= 80))
            {
                tempDict.Add(item.Key, Truncate(item.Value, 125));
            }
        }
        
        await LogEvent(ex.Message, ex.StackTrace, LogType.Crash, ex, JsonConvert.SerializeObject(tempDict));
    }
    
    public static async Task TrackError(string title, string message, LogType logType)
    {
        await LogEvent(title, message, logType);
    }

    private static async Task LogEvent(string? title, string? message, LogType logType, Exception? exception = null, string properties = null)
    {
        var logService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
        
        var description = exception != null ? exception.Message : message;
        var titleText = exception != null
            ? !string.IsNullOrEmpty(exception.StackTrace) 
                ? exception.StackTrace 
                : title 
            : title;
        
        var log = new Log
        {   
            Id = Guid.NewGuid(),
            AppVersionBuild = $"{AppInfo.Current.VersionString} ({AppInfo.Current.BuildString})",
            StackTrace = exception?.StackTrace,
            Description = Truncate(description, 80),
            Title = Truncate(titleText, 80) ,
            Properties = properties,
            Timestamp = DateTime.Now,
            Type = logType
        };
        
        await logService?.LogEventAsync(log);
    }
    
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}

public enum LogType
{
    Debug,
    Information,
    Warning,
    Error,
    Crash
}