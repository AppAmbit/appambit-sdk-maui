using System.Diagnostics;
using System.Runtime.CompilerServices;
using AppAmbit.Enums;
using AppAmbit.Models.Logs;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using AppAmbit.Enums;
using Newtonsoft.Json;
using Shared.Utils;

namespace AppAmbit;

public static class Crashes
{
    public static event Action<object> OnCrashException;
    private static IStorageService? _storageService;
    private static IAPIService? _apiService;
    private static string _deviceId;
    private static readonly SemaphoreSlim _ensureFileLocked = new SemaphoreSlim(1,1);

    internal static void Initialize(IAPIService? apiService, IStorageService? storageService, string deviceId)
    {
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= UnobservedTaskException;
        TaskScheduler.UnobservedTaskException += UnobservedTaskException;

        _storageService = storageService;
        _apiService = apiService;
        _deviceId = deviceId;
        Logging.Initialize(apiService, storageService);
    }

    public static async Task LogError(Exception? exception, Dictionary<string, string> properties = null, string? classFqn = null, [CallerFilePath] string? fileName = null, [CallerLineNumber] int lineNumber = 0)
    {
        classFqn = classFqn ?? await GetCallerClassAsync();
        await Logging.LogEvent("", LogType.Error, exception, properties, classFqn, fileName, lineNumber);
    }

    public static async Task LogError(string message, Dictionary<string, string> properties = null, string? classFqn = null, Exception? exception = null, [CallerFilePath] string? fileName = null, [CallerLineNumber] int? lineNumber = null, DateTime? createdAt = null)
    {
        classFqn = classFqn ?? await GetCallerClassAsync();
        await Logging.LogEvent(message, LogType.Error, exception, properties, classFqn, fileName, lineNumber, createdAt);
    }

    public static async Task GenerateTestCrash()
    {
        throw new NullReferenceException();
    }

    internal static async Task LoadCrashFileIfExists()
    {
        // This semaphore ensures mutual exclusion
        // between onStart and onConnectivityChanged
        await _ensureFileLocked.WaitAsync();
        try
        {
            if (!SessionManager.IsSessionActive)
            {
                return;
            }
            var crashFiles = Directory.EnumerateFiles(FileSystem.AppDataDirectory, "crash_*.json", SearchOption.TopDirectoryOnly);
            int crashFileCount = crashFiles.Count();
            if (crashFileCount == 0)
            {
                SetCrashFlag(false);
                return;
            }

            Debug.WriteLine($"Debug Count of Crashes: {crashFileCount}");

            SetCrashFlag(true);

            var exceptionInfos = new List<ExceptionInfo>();

            foreach (var file in crashFiles)
            {
                var exceptionInfo = await ReadAndDeleteCrashFileAsync(file);
                if (exceptionInfo != null)
                {
                    exceptionInfos.Add(exceptionInfo);
                }
            }
            
            if (crashFileCount == 1)
            {
                Debug.WriteLine($"Sending one crash {exceptionInfos.Count} crash files");
                await LogCrash(exceptionInfos[0]);
                DeleteCrashes();
            }
            else if (crashFileCount > 1)
            {
                Debug.WriteLine($"Sending crash batch: {exceptionInfos.Count} items");
                await StoreBatchCrashesLog(exceptionInfos);
            }
        }
        finally
        {
            _ensureFileLocked.Release();
        }
    }

    public static async Task<bool> DidCrashInLastSession()
    {
        var path = GetCrashFilePath(CrashFileType.DidAppCrash);
        return CrashFileExists(path);
    }

    private static async Task LogCrash(ExceptionInfo? exception = null)
    {
        var message = exception?.Message;
        await Logging.LogEvent(message, LogType.Crash, exception);
    }

    private static async Task LogError(ExceptionInfo? exception = null)
    {
        var message = exception?.Message;
        await Logging.LogEvent(message, LogType.Error, exception);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    private static async void UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        var exception = ExceptionInfo.FromException(e?.Exception);
        await LogError(exception);
    }

    private static async void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        if (unhandledExceptionEventArgs.ExceptionObject is not Exception ex)
            return;

        var info = ExceptionInfo.FromException(ex, _deviceId);
        var json = JsonConvert.SerializeObject(info, Formatting.Indented);

        SaveCrashToFile(json);

    OnCrashException?.Invoke(ex);
    }
    private static void SaveCrashToFile(string json)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"crash_{timestamp}.json";

        string crashFile = Path.Combine(FileSystem.AppDataDirectory, fileName);

        Debug.WriteLine($"Crash file saved to: {crashFile}");
        File.WriteAllText(crashFile, json);
    }

    private static async Task<string?> GetCallerClassAsync()
    {
        var listSystemNames = new List<string>()
        {
            $"System.",
            $"AppAmbit.",
            $"Foundation.",
            $"UIKit.",
        };
        await Task.Yield();
        var stackTrace = new StackTrace();
        var classFqn = (string?)null;
        foreach (var frame in stackTrace.GetFrames())
        {
            var method = frame?.GetMethod();
            var fullName = method?.DeclaringType?.FullName ?? "";
            if (!ContainsFromList(fullName, listSystemNames))
            {
                classFqn = fullName;
            }
        }
        return classFqn;
    }

    private static bool ContainsFromList(string word, List<string> list)
    {
        foreach (var s in list)
        {
            if (word.Contains(s))
                return true;
        }
        return false;
    }

    public static async Task SendBatchLogs()
    {
      Debug.WriteLine("SendBatchLogs");
        var logEntityList = await _storageService.GetOldest100LogsAsync();
        if (logEntityList?.Count == 0)
        {
            Debug.WriteLine("No logs to send");
            return;
        }

        Debug.WriteLine($"Debug EntityList Content: {logEntityList}");

        Debug.WriteLine("Sending logs in batch");
        var logBatch = new LogBatch() { Logs = logEntityList };
        var endpoint = new LogBatchEndpoint(logBatch);
        var logResponse = await _apiService?.ExecuteRequest<Response>(endpoint);
        if (logResponse?.ErrorType != ApiErrorType.None)
        {
            Debug.WriteLine($"Batch of unsent logs");
            return;
        }

        await _storageService.DeleteLogList(logEntityList);
        Debug.WriteLine("Logs batch sent");
    }

    public static async Task StoreBatchCrashesLog(List<ExceptionInfo> crashList)
    {
        Debug.WriteLine("Debug Storing in DB Crashes Batches");
        foreach (var crash in crashList)
        {
            try
            {
                var logEntity = MapExceptionInfoToLogEntity(crash);
                Debug.WriteLine("Debug LogEntity: " + logEntity);
                logEntity.Id = Guid.NewGuid();
                if (crash.CreatedAt == default)
                {
                    logEntity.CreatedAt = DateUtils.GetUtcNow;
                }
                else
                {
                    logEntity.CreatedAt = crash.CreatedAt;
                }
                if (_storageService == null)
                {
                    return;
                }
                await _storageService.LogEventAsync(logEntity);
            }
            catch(Exception e)
            {
                Debug.WriteLine("Debug exception: " + e);
            }
        }
        DeleteCrashes();
    }

    private static void DeleteCrashes()
    {
        var crashFiles = Directory.EnumerateFiles(FileSystem.AppDataDirectory, "crash_*.json", SearchOption.TopDirectoryOnly);
        foreach (var crashFile in crashFiles)
        {
            File.Delete(crashFile);
        }
        Debug.WriteLine("Debug all crashes deleted");
    }

    private static LogEntity MapExceptionInfoToLogEntity(ExceptionInfo exception, LogType logType = LogType.Crash)
    {
        var file = exception?.CrashLogFile;
        return new LogEntity
        {
            AppVersion = $"{AppInfo.VersionString} ({AppInfo.BuildString})",
            ClassFQN = exception?.ClassFullName ?? AppConstants.UnknownClass,
            FileName = exception?.FileNameFromStackTrace ?? AppConstants.UnknownFileName,
            LineNumber = exception?.LineNumberFromStackTrace ?? 0,
            Message = exception?.Message ?? "",
            StackTrace = exception?.StackTrace,
            Context = new Dictionary<string, string>
        {
            { "Source", exception?.Source ?? "" },
            { "InnerException", exception?.InnerException ?? "" }
        },
            Type = logType,
            File = (logType == LogType.Crash && exception != null ? file : null),
            CreatedAt = exception.CreatedAt
        };
    }

    private static string GetCrashFilePath(CrashFileType type)
    {
        string fileName = type switch
        {
            CrashFileType.LastCrash => "last_crash.json",
            CrashFileType.DidAppCrash => "did_app_crash.json",
            _ => throw new ArgumentOutOfRangeException()
        };
        return Path.Combine(FileSystem.AppDataDirectory, fileName);
    }

    private static bool CrashFileExists(string path)
    {
        return File.Exists(path);
    }

    private static void SetCrashFlag(bool didCrash)
    {
        var path = GetCrashFilePath(CrashFileType.DidAppCrash);
        if (!didCrash)
        {
            File.Delete(path);
            return;
        }
        try
        {
            File.WriteAllText(path, String.Empty);
        }
        catch
        {
            return;
        }
    }

    private static async Task<ExceptionInfo?> ReadAndDeleteCrashFileAsync(string path)
    {
        try
        {
            var json = await File.ReadAllTextAsync(path);
            return JsonConvert.DeserializeObject<ExceptionInfo>(json);
        }
        catch
        {
            return null;
        }
    }
}