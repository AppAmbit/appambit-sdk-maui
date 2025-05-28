using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using AppAmbit.Enums;
using Shared.Utils;
using System.Diagnostics;

namespace AppAmbit;

internal static class Logging
{
    private static IAPIService? _apiService;
    private static IStorageService? _storageService;
    public static void Initialize(IAPIService? apiService, IStorageService? storageService)
    {
        _apiService = apiService;
        _storageService = storageService;
    }

    public static async Task LogEvent(string? message, LogType logType, Exception? exception = null, Dictionary<string, string>? properties = null, string? classFqn = null, string? fileName = null, int? lineNumber = null)
    {
        var deviceId = await _storageService.GetDeviceId();
        var exceptionInfo = (exception != null) ? ExceptionInfo.FromException(exception, deviceId) : null;
        LogEvent(message, logType, exceptionInfo, properties, classFqn, fileName, lineNumber);
    }

    public static async Task LogEvent(string? message, LogType logType, ExceptionInfo exception = null, Dictionary<string, string>? properties = null, string? classFqn = null, string? fileName = null, int? lineNumber = null)
    {
        if (!SessionManager.IsSessionActive)
            return;

        var stackTrace = exception?.StackTrace;
        stackTrace = string.IsNullOrEmpty(stackTrace) ? AppConstants.NoStackTraceAvailable : stackTrace;
        var file = exception?.CrashLogFile;
        var log = new Log
        {
            AppVersion = $"{AppInfo.VersionString} ({AppInfo.BuildString})",
            ClassFQN = exception?.ClassFullName ?? classFqn ?? AppConstants.UnknownClass,
            FileName = exception?.FileNameFromStackTrace ?? fileName ?? AppConstants.UnknownFileName,
            LineNumber = exception?.LineNumberFromStackTrace ?? lineNumber ?? 0,
            Message = exception?.Message ?? (string.IsNullOrEmpty(message) ? "" : message),
            StackTrace = stackTrace,
            Context = properties ?? new Dictionary<string, string>(),
            Type = logType,
            File = (logType == LogType.Crash && exception != null) ? file : null,
            CreatedAt = DateUtils.GetUtcNow,
        };
        await SendOrSaveLogEventAsync(log);
    }

    private static async Task SendOrSaveLogEventAsync(Log log)
    {
        var logEndpoint = new LogEndpoint(log);

        try
        {
            var logResponse = await _apiService?.ExecuteRequest<LogResponse>(logEndpoint);

            if (logResponse == null || logResponse.ErrorType != ApiErrorType.None)
            {
                await StoreLogInDb(log);
                return;
            }
        }
        catch (Exception ex)
        {
           Debug.WriteLine($"Error {ex.Message}");
        }
    }


    private static async Task StoreLogInDb(Log log)
    {
        var logEntity = log.ConvertTo<LogEntity>();
        logEntity.Id = Guid.NewGuid();
        logEntity.CreatedAt = DateUtils.GetUtcNow;

        await _storageService?.LogEventAsync(logEntity);
    }
}