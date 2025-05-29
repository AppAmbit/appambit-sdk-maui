using Shared.Utils;

namespace AppAmbit.Models.Logs;

public class ExceptionInfo
{
    public string Type { get; set; }
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
        
        return new ExceptionInfo
        {
            Type = exception?.GetType()?.FullName,
            Message = exception?.Message,
            StackTrace = exception?.StackTrace,
            Source = exception?.Source,
            InnerException = exception?.InnerException?.ToString(),
            ClassFullName = exception?.TargetSite?.DeclaringType?.FullName ?? AppConstants.UnknownClass,
            FileNameFromStackTrace = exception?.GetFileNameFromStackTrace() ?? AppConstants.UnknownFileName,
            LineNumberFromStackTrace = exception?.GetLineNumberFromStackTrace() ?? 0,
            CrashLogFile = CrashFileGenerator.GenerateCrashLog(exception,deviceId),
            CreatedAt = DateUtils.GetUtcNow
        };
    }
}