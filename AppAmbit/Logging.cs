using AppAmbit.Models.Logs;
using AppAmbit.Services.Interfaces;

namespace AppAmbit;

public static class Logging
{
    public static async Task LogAsync(string title, string message, LogType logType)
    {
        var logService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();

        var log = new Log
        {   
            Id = Guid.NewGuid(),
            AppVersion = AppInfo.Current.VersionString,
            Message = message,
            Timestamp = DateTime.Now,
            Title = title,
            Type = logType
        };
        
        await logService?.LogEventAsync(log);
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