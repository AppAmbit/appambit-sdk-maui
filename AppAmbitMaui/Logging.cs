using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;

namespace AppAmbit;

internal static class Logging
{
    private static IAPIService? _apiService;
    private static IStorageService? _storageService;
    public static void Initialize(IAPIService? apiService,IStorageService? storageService)
    {
        _apiService = apiService;
        _storageService = storageService;
    }

    public static async Task LogEvent(string? message, LogType logType, Exception? exception = null, Dictionary<string,string>? properties = null,string? classFqn = null, string? fileName = null, int? lineNumber = null)
    {
        var stackTrace = exception?.StackTrace;
        stackTrace = (String.IsNullOrEmpty(stackTrace)) ? AppConstants.NoStackTraceAvailable : stackTrace;
        var deviceId = await _storageService.GetDeviceId();
        var file = (exception != null) ? CrashFileGenerator.GenerateCrashLog(exception,deviceId) : null;
        var log = new Log
        {
            AppVersion = $"{AppInfo.VersionString} ({AppInfo.BuildString})",
            ClassFQN = exception?.TargetSite?.DeclaringType?.FullName ?? classFqn ?? AppConstants.UnknownClass,
            FileName = exception?.GetFileNameFromStackTrace() ?? fileName ??  AppConstants.UnknownFileName,
            LineNumber = exception?.GetLineNumberFromStackTrace() ?? lineNumber ??  0,
            Message = exception?.Message ?? ( String.IsNullOrEmpty(message) ?  "" : message),
            StackTrace = stackTrace,
            Context = properties ?? new Dictionary<string,string>(),
            Type = logType,
            file = file,
        };
        await SendOrSaveLogEventAsync(log);
    }
    
    private static async Task SendOrSaveLogEventAsync(Log log)
    {
        bool hasInternet() => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        var token = _apiService?.GetToken();
        
        //Check the token to see if maybe the consumer api has not been completed yet, so we need to wait to send the log.
        if (hasInternet() && !string.IsNullOrEmpty(token) && log.Type!=LogType.Crash)
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
                    var logResponse = await _apiService?.ExecuteRequest<LogResponse>(registerEndpoint);
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
                await StoreLogInDb(log);
            }
        }
        else
        {
            await StoreLogInDb(log);
        }
    }

    private static async Task StoreLogInDb(Log log)
    {
        var logEntity = log.ConvertTo<LogEntity>();
        logEntity.Id = Guid.NewGuid();
        logEntity.CreatedAt = DateTime.Now.ToUniversalTime();
        
        await _storageService?.LogEventAsync(logEntity);
    }
}