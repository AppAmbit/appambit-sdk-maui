using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;


namespace AppAmbit;

internal class Logging
{
    
    public static async Task LogEvent(string? message, LogType logType, Exception? exception = null, Dictionary<string, object>? properties = null)
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
            Context = properties ?? new Dictionary<string, object>(),
            Type = logType
        };
        await SendOrSaveLogEventAsync(log);
    }
    
    private static async Task SendOrSaveLogEventAsync(Log log)
    {
        bool hasInternet() => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        var storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>()
                             ?? throw new InvalidOperationException("StorageService is not available.");
        var token = await storageService.GetToken();
        
        //Check the token to see if maybe the consumer api has not been completed yet, so we need to wait to send the log.
        if (hasInternet() && !string.IsNullOrEmpty(token))
        {
            var registerEndpoint = new LogEndpoint(log);
            
            var retryCount = 0;
            var maxRetryCount = 3;
            const int delayMilliseconds = 500;
            var hasErrors = false;
            var hasCompleted = false;
            do{
                try
                {
                    var apiService = Application.Current?.Handler?.MauiContext?.Services.GetService<IAPIService>()
                                     ?? throw new InvalidOperationException("APIService is not available.");
                    var logResponse = await apiService?.ExecuteRequest<LogResponse>(registerEndpoint);
                    hasCompleted = true;
                }
                catch (Exception ex)
                {
                    hasErrors = true;
                    
                    if (retryCount < maxRetryCount)
                    {
                        await Task.Delay(delayMilliseconds);
                    }
                }
            } while( hasErrors && hasInternet() && retryCount++ < maxRetryCount  );

            if (!hasCompleted)
            {
                await StoreLogInDb(log, storageService);
            }
        }
        else
        {
            await StoreLogInDb(log, storageService);
        }
    }

    private static async Task StoreLogInDb(Log log,IStorageService storeService )
    {
        var logTimestamp = log.ConvertTo<LogTimestamp>();
        logTimestamp.Id = Guid.NewGuid();
        logTimestamp.Timestamp = DateTime.Now.ToUniversalTime();
        
        await storeService.LogEventAsync(logTimestamp);
    }
}