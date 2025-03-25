using System.Net.Mime;
using iOSAppAmbit.Services;
using iOSAppAmbit.Services.Base;
using Shared.Models.Logs;
using Microsoft.Extensions.DependencyInjection;

namespace iOSAppAmbit;

public static class Logging
{
    public static async Task LogAsync(string title, string message, LogType logType)
    {
        var logService = Core.Services.GetService<IStorageService>();

        var log = new Log
        {   
            Id = Guid.NewGuid(),
            AppVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString")?.ToString(),
            Message = message,
            Timestamp = DateTime.Now,
            Title = title,
            Type = logType 
        };
        
        await logService?.LogEventAsync(log);
    }
}