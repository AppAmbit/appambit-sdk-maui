using System.Diagnostics;
using System.Runtime.CompilerServices;
using AppAmbit.Enums;
using AppAmbit.Models.Logs;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit
{
    public static class Crashes
    {
        public static event Action<object> OnCrashException;
        private static IStorageService? _storageService;
        private static IAPIService? _apiService;
        private static string _deviceId = "";
        private static readonly SemaphoreSlim _ensureFileLocked = new SemaphoreSlim(1, 1);

        internal static void Initialize(IAPIService? apiService, IStorageService? storageService, string deviceId)
        {
            AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException -= UnobservedTaskException;
            TaskScheduler.UnobservedTaskException += UnobservedTaskException;

            _storageService = storageService;
            _apiService = apiService;
            _deviceId = deviceId ?? "";
            Logging.Initialize(apiService, storageService, _deviceId);
        }

        public static Task LogError(
            Exception? exception,
            Dictionary<string, string>? properties = null,
            string? classFqn = null,
            [CallerFilePath] string? fileName = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            classFqn ??= GetCallerClass();
            return Logging.LogEvent("", LogType.Error, exception, properties, classFqn, fileName, lineNumber);
        }

        public static async Task LogError(
            string message,
            Dictionary<string, string>? properties = null,
            string? classFqn = null,
            Exception? exception = null,
            [CallerFilePath] string? fileName = null,
            [CallerLineNumber] int? lineNumber = null)
        {
            try
            {
                classFqn ??= GetCallerClass();
                await Logging.LogEvent(message, LogType.Error, exception, properties, classFqn, fileName, lineNumber);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Crashes] Exception during LogError: {ex}");
            }
        }

        public static Task GenerateTestCrash()
        {
            throw new NullReferenceException();
        }

        internal static async Task LoadCrashFileIfExists()
        {
            await _ensureFileLocked.WaitAsync();
            try
            {
                if (!SessionManager.IsSessionActive)
                    return;

                var crashFiles = Directory.EnumerateFiles(AppPaths.AppDataDir, "crash_*.json", SearchOption.TopDirectoryOnly);
                int crashFileCount = crashFiles != null ? crashFiles.Count() : 0;


                if (crashFileCount == 0)
                {
                    SetCrashFlag(false);
                    return;
                }

                Debug.WriteLine($"Debug Count of Crashes: {crashFileCount}");
                SetCrashFlag(true);

                var exceptionInfos = new List<ExceptionInfo>();

                foreach (var file in System.IO.Directory.EnumerateFiles(AppPaths.AppDataDir, "crash_*.json", System.IO.SearchOption.TopDirectoryOnly))
                {
                    var exceptionInfo = await ReadAndDeleteCrashFileAsync(file);
                    if (exceptionInfo != null)
                        exceptionInfos.Add(exceptionInfo);
                }

                if (exceptionInfos.Count == 1)
                {
                    Debug.WriteLine($"Sending one crash {exceptionInfos.Count} crash files");
                    await LogCrash(exceptionInfos[0]);
                    DeleteCrashes();
                }
                else if (exceptionInfos.Count > 1)
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

        public static Task<bool> DidCrashInLastSession()
        {
            try
            {
                var exists = File.Exists(Path.Combine(AppPaths.AppDataDir, AppConstants.DidCrashFileName));
                return Task.FromResult(exists);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private static Task LogCrash(ExceptionInfo? exception = null)
        {
            return Logging.LogEventFromExceptionInfo(exception?.Message, LogType.Crash, exception);
        }

        private static Task LogError(ExceptionInfo? exception = null)
        {
            return Logging.LogEventFromExceptionInfo(exception?.Message, LogType.Error, exception);
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        private static async void UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            if (!Analytics._isManualSessionEnabled)
                SessionManager.SaveEndSession();

            if (!SessionManager.IsSessionActive)
                return;

            var exception = ExceptionInfo.FromException(e?.Exception, _deviceId);
            await LogError(exception);
        }

        private static async void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            if (!Analytics._isManualSessionEnabled)
                SessionManager.SaveEndSession();

            if (!SessionManager.IsSessionActive)
                return;

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
            string crashFile = Path.Combine(AppPaths.AppDataDir, fileName);

            Debug.WriteLine($"Crash file saved to: {crashFile}");
            File.WriteAllText(crashFile, json);
        }

        private static string? GetCallerClass()
        {
            var listSystemNames = new List<string> { "System.", "AppAmbit.", "Foundation.", "UIKit." };
            var stackTrace = new StackTrace();
            string? classFqn = null;
            var frames = stackTrace.GetFrames();
            if (frames == null) return classFqn;
            foreach (var frame in frames)
            {
                var method = frame?.GetMethod();
                var fullName = method?.DeclaringType?.FullName ?? "";
                if (!ContainsFromList(fullName, listSystemNames))
                    classFqn = fullName;
            }
            return classFqn;
        }

        private static bool ContainsFromList(string word, List<string> list)
        {
            foreach (var s in list)
                if (word.Contains(s))
                    return true;
            return false;
        }

        internal static async Task SendBatchLogs()
        {
            try
            {
                Debug.WriteLine("Send Batch Logs");
                var list = await _storageService?.GetOldest100LogsAsync()!;
                if (list == null || list.Count == 0)
                {
                    Debug.WriteLine($"No logs to send");
                    return;
                }

                var logBatch = new LogBatch() { Logs = list };
                var endpoint = new LogBatchEndpoint(logBatch);
                var logResponse = await _apiService?.ExecuteRequest<Response>(endpoint)!;
                if (logResponse == null || logResponse.ErrorType != ApiErrorType.None)
                {
                    Debug.WriteLine("Batch of unsent logs");
                    return;
                }

                await _storageService!.DeleteLogList(list);
                Debug.WriteLine("Finished Logs Batch");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Crashes] Exception during SendBatchLogs: {ex}");
            }
        }

        private static async Task StoreBatchCrashesLog(List<ExceptionInfo> crashList)
        {
            Debug.WriteLine("Debug Storing in DB Crashes Batches");
            foreach (var crash in crashList)
            {
                try
                {
                    var logEntity = MapExceptionInfoToLogEntity(crash);
                    logEntity.Id = Guid.NewGuid();
                    logEntity.CreatedAt = (crash.CreatedAt == default) ? DateTime.UtcNow : crash.CreatedAt;
                    if (_storageService == null) return;
                    await _storageService.LogEventAsync(logEntity);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Debug exception: " + e);
                }
            }
            DeleteCrashes();
        }

        private static void DeleteCrashes()
        {
            foreach (var crashFile in Directory.EnumerateFiles(AppPaths.AppDataDir, "crash_*.json", System.IO.SearchOption.TopDirectoryOnly))
                File.Delete(crashFile);
            Debug.WriteLine("Debug all crashes deleted");
        }

        private static LogEntity MapExceptionInfoToLogEntity(ExceptionInfo exception, LogType logType = LogType.Crash)
        {
            var file = exception?.CrashLogFile;
            return new LogEntity
            {
                SessionId = exception?.SessionId,
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
                File = (logType == LogType.Crash && exception != null) ? file : null,
                CreatedAt = exception.CreatedAt
            };
        }

        private static string GetCrashFilePath()
        {
            return Path.Combine(AppPaths.AppDataDir, AppConstants.DidCrashFileName);
        }

        private static void SetCrashFlag(bool didCrash)
        {
            var path = GetCrashFilePath();
            if (!didCrash)
            {
                File.Delete(path);
                return;
            }
            try
            {
                File.WriteAllText(path, string.Empty);
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
                var json = await System.IO.File.ReadAllTextAsync(path);
                return JsonConvert.DeserializeObject<ExceptionInfo>(json);
            }
            catch
            {
                return null;
            }
        }
    }
}
