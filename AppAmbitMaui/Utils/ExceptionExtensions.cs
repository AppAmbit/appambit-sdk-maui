using System.Diagnostics;

namespace AppAmbit;

public static class ExceptionExtensions
{
    public static string GetFileNameFromStackTrace(this Exception ex)
    {
        var stackFrames = new StackTrace(ex, true).GetFrames();
        if (stackFrames == null || stackFrames.Length == 0)
        {
            return AppConstants.UnknownFileName;
        }
        var stackFrame = new StackTrace(ex, true).GetFrames().Last();
        return stackFrame?.GetFileName() ?? AppConstants.UnknownFileName;
    }

    public static long GetLineNumberFromStackTrace(this Exception ex)
    {
        var stackFrames = new StackTrace(ex, true).GetFrames();
        if (stackFrames == null || stackFrames.Length == 0)
        {
            return 0;
        }
        var stackFrame = new StackTrace(ex, true).GetFrames().Last();
        return stackFrame?.GetFileLineNumber() > 0 ? stackFrame.GetFileLineNumber() : 0;
    }
}