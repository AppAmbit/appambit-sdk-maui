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
                tempDict.Add(item.Key, item.Value.Truncate( 125));
            }
        }
        
        await LogEvent( ex?.StackTrace, LogType.Crash, ex, JsonConvert.SerializeObject(tempDict));
    }
    
    public static async Task LogError(string message, LogType logType)
    {
        await LogEvent(message, logType);
    }

    public static async Task GenerateTestCrash()
    {
        await LogEvent( "This is a test crash", LogType.Crash);
    }
    
    private static async Task LogEvent(string? message, LogType logType, Exception? exception = null, string properties = null)
    {
        var stackTrace = exception?.StackTrace;
        stackTrace = (String.IsNullOrEmpty(stackTrace)) ? AppConstants.NO_STACKTRACE_AVAILABLE : stackTrace;
        var log = new Log
        {
            Id = Guid.NewGuid(),
            AppVersion = $"{AppInfo.VersionString} ({AppInfo.BuildString})",
            ClassFQN = exception?.TargetSite?.DeclaringType?.FullName ?? AppConstants.UNKNOWNCLASS,
            FileName = exception?.GetFileNameFromStackTrace() ?? AppConstants.UNKNOWNFILENAME,
            LineNumber = exception?.GetLineNumberFromStackTrace() ?? 0,
            Message = "" + message,
            StackTrace = stackTrace,
            Context = new Dictionary<string, object>(),
            Type = logType
        };
                
        var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        var token = await Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>()?.GetToken();
        //Check the token to see if maybe the consumer api has not been completed yet, so we need to wait to send the log.
        if (hasInternet && !string.IsNullOrEmpty(token))
        {
            var apiService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>();
            var registerEndpoint = new LogEndpoint(log);
            var logResponse = await apiService?.ExecuteRequest<LogResponse>(registerEndpoint);
        }
        else
        {
            var storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
            await storageService?.LogEventAsync(log);
        }
    }
}