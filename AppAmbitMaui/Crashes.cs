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
        await SendOrSaveLogEventAsync(log);
    }
    
    private static async Task SendOrSaveLogEventAsync(Log log)
    {    
        var hasInternet = ()=> Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        var token = await Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>()?.GetToken();
        
        //Check the token to see if maybe the consumer api has not been completed yet, so we need to wait to send the log.
        if (hasInternet() && !string.IsNullOrEmpty(token))
        {
            var apiService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>();
            var registerEndpoint = new LogEndpoint(log);
            
            var retryCounter = 0;
            var hasErrors = false;
            var hasCompleted = false;
            do{
                try
                {
                    var logResponse = await apiService?.ExecuteRequest<LogResponse>(registerEndpoint);
                    hasCompleted = true;
                }
                catch (Exception ex)
                {
                    hasErrors = true;
                }
            } while( hasErrors && hasInternet() && retryCounter++ < 3  );

            if (!hasCompleted)
            {
                await StoreLogInDB(log);
            }
        }
        else
        {
            await StoreLogInDB(log);
        }
    }

    private static async Task StoreLogInDB(Log log)
    {
        var logTimestamp = log.ConvertTo<LogTimestamp>();
        logTimestamp.Id = Guid.NewGuid();
        logTimestamp.Timestamp = DateTime.Now.ToUniversalTime();
        
        var storeService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
        await storeService.LogEventAsync(logTimestamp);
    }
    
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}