using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
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

    public static async Task GenerateTestCrash()
    {
        await LogEvent("Test Crash", "This is a test crash", LogType.Crash);
        await Core.SendSummaryAndFile();
    }
    
    private static async Task LogEvent(string? title, string? message, LogType logType, Exception? exception = null, string properties = null)
    {
        var description = exception != null ? exception.Message : message;
        var titleText = exception != null
            ? !string.IsNullOrEmpty(exception.StackTrace) 
                ? exception.StackTrace 
                : title 
            : title;
            
        var log = new Log
        {
            Id = Guid.NewGuid(),
            AppVersion = "1.0.0",
            ClassFQN = @"App\Http\Controllers\Api\LogController",
            FileName = "Microsoft.Maui.Controls.Button",
            LineNumber = 23,
            Message = "Call to a member function get() on null",
            StackTrace = "Stacktrace:"+ exception?.StackTrace,
            Context = new Dictionary<string, object>()
            {
                { "user_id", 1 }
            },
            Type = "error",
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