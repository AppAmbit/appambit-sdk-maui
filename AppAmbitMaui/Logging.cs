using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Process = System.Diagnostics.Process;

#if ANDROID
using Android.OS;
#elif IOS
using UIKit;
#endif


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
        var logTimestamp = log.ConvertTo<LogTimestamp>();
        logTimestamp.Id = Guid.NewGuid();
        logTimestamp.Timestamp = DateTime.Now.ToUniversalTime();
        
        await _storageService?.LogEventAsync(logTimestamp);
    }
    
    
    
    private static string GenerateCrashLog(Exception ex)
    {
        StringBuilder log = new StringBuilder();

        // Basic App & Device Info
        log.AppendLine($"Package: {AppInfo.PackageName}");
        log.AppendLine($"Version Code: {AppInfo.BuildString}");
        log.AppendLine($"Version Name: {AppInfo.VersionString}");
        
#if ANDROID
        log.AppendLine($"Android: {Build.VERSION.SdkInt}");
        log.AppendLine($"Android Build: {Build.Display}");
        log.AppendLine($"Manufacturer: {Build.Manufacturer}");
        log.AppendLine($"Model: {Build.Model}");
#elif IOS
        log.AppendLine($"iOS: {UIDevice.CurrentDevice.SystemVersion}");
        log.AppendLine($"Device: {UIDevice.CurrentDevice.Model}");
        log.AppendLine($"Manufacturer: Apple");
        log.AppendLine($"Model: {DeviceInfo.Model}");
#endif

        log.AppendLine($"CrashReporter Key: {Guid.NewGuid()}"); // Simulated unique crash key
        log.AppendLine($"Start Date: {DateTime.UtcNow.AddSeconds(-20):O}");
        log.AppendLine($"Date: {DateTime.UtcNow:O}");
        log.AppendLine();

        // Exception Stack Trace
        log.AppendLine("Xamarin Exception Stack:");
        log.AppendLine(ex.ToString());
        log.AppendLine();
        
        addThreadsInfo(log);

        return log.ToString();
    }
    
    public static void addThreadsInfo(StringBuilder log) {
        
        // Log all running threads
        //log.AppendLine("Active Threads Stack Traces:");
        foreach (var thread in Process.GetCurrentProcess().Threads.Cast<ProcessThread>())
        {
            try
            {
                log.AppendLine($"Thread {thread.Id}: {thread.ThreadState}");
            }
            catch (Exception ex)
            {
                log.AppendLine($"Failed to get thread info: {ex.Message}");
            }
        }
    }
}