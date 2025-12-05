namespace AppAmbitSdkCore.Models.Logs;

public class ExceptionInfo
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

    public DateTime CreatedAt {  get; set; }

    public static ExceptionInfo FromException(Exception exception, string deviceId = null)
    {
        var info = new ExceptionInfo();

        info.Type = exception?.GetType()?.FullName;
        info.Message = exception?.Message;
        info.StackTrace = exception?.StackTrace;
        info.Source = exception?.Source;
        info.InnerException = exception?.InnerException?.ToString();
        info.ClassFullName = exception?.TargetSite?.DeclaringType?.FullName ?? AppConstants.UnknownClass;
        info.FileNameFromStackTrace = exception?.GetFileNameFromStackTrace() ?? AppConstants.UnknownFileName;
        info.LineNumberFromStackTrace = exception?.GetLineNumberFromStackTrace() ?? 0;
        info.CrashLogFile = CrashFileGenerator.GenerateCrashLog(exception, deviceId);
        info.SessionId = SessionManager.SessionId;
        info.CreatedAt = DateTime.UtcNow;

        return info;
    }
}