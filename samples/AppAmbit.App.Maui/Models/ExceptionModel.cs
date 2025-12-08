
using System.Diagnostics;
using System.Text;


#if ANDROID
using Java.Lang;
using Exception = System.Exception;
using Process = System.Diagnostics.Process;
using StringBuilder = System.Text.StringBuilder;
#endif
namespace AppAmbitTestingApp.Models;

public class ExceptionModelApp
{
    public string Type { get; set; }
    public string SessionId { get; set; }
    public string Message { get; set; }
    public string StackTrace { get; set; }
    public string Source { get; set; }
    public string InnerException { get; set; }
    public string FileNameFromStackTrace { get; set; }

    public string ClassFullName { get; set; }

    public long LineNumberFromStackTrace { get; set; }

    public string CrashLogFile { get; set; }

    public DateTime CreatedAt { get; set; }

    public static ExceptionModelApp FromException(Exception exception, string deviceId, string sessionId)
    {

        return new ExceptionModelApp
        {
            Type = exception.GetType().FullName ?? "UnknownType",
            Message = exception.Message,
            StackTrace = exception.StackTrace ?? "NoStackTrace",
            Source = exception.Source ?? "UnknownSource",
            InnerException = exception.InnerException?.ToString() ?? "NoInnerException",
            ClassFullName = exception.TargetSite?.DeclaringType?.FullName ?? "UnknownClass",
            FileNameFromStackTrace = GetFileNameFromStackTrace(exception) ?? "UnknownFileName",
            LineNumberFromStackTrace = GetLineNumberFromStackTrace(exception),
            CrashLogFile = GenerateCrashLog(exception, deviceId),
            SessionId = sessionId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static string GetFileNameFromStackTrace(Exception ex)
    {
        var stackFrames = new StackTrace(ex, true).GetFrames();
        if (stackFrames == null || stackFrames.Length == 0)
        {
            return "UnknownFileName";
        }
        var stackFrame = new StackTrace(ex, true).GetFrames().Last();
        return stackFrame?.GetFileName() ?? "UnknownFileName";
    }

    public static long GetLineNumberFromStackTrace(Exception ex)
    {
        var stackFrames = new StackTrace(ex, true).GetFrames();
        if (stackFrames == null || stackFrames.Length == 0)
        {
            return 0;
        }
        var stackFrame = new StackTrace(ex, true).GetFrames().Last();
        return stackFrame?.GetFileLineNumber() > 0 ? stackFrame.GetFileLineNumber() : 0;
    }

    public static string GenerateCrashLog(Exception ex, string deviceId)
    {
        StringBuilder log = new StringBuilder();

#if ANDROID
        CrashFileGeneratorAndroidApp.AddHeader(log, deviceId);
#elif IOS
        CrashFileGeneratorIOSApp.AddHeader(log,deviceId);
#endif

        log.AppendLine();
        log.AppendLine("Xamarin Exception Stack:");
        log.AppendLine(ex.StackTrace);
        log.AppendLine();

#if ANDROID
        CrashFileGeneratorAndroidApp.AddThreads(log);
#elif IOS
        CrashFileGeneratorIOSApp.AddThreads(log);
#else 
        AddThreads(log);
#endif

        return log.ToString();
    }

    private static void AddThreads(StringBuilder log)
    {
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
