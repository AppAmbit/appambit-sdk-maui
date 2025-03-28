using System.Diagnostics;

namespace AppAmbit;

public static class ExceptionExtensions
{
    public static string GetFileNameFromStackTrace(this Exception ex)
    {
        var stackFrame = new StackTrace(ex, true).GetFrame(0);
        return stackFrame?.GetFileName() ?? AppConstants.UNKNOWNFILENAME;
    }

    public static long GetLineNumberFromStackTrace(this Exception ex)
    {
        var stackFrame = new StackTrace(ex, true).GetFrame(0);
        return stackFrame?.GetFileLineNumber() > 0 ? stackFrame.GetFileLineNumber() : 0;
    }
}