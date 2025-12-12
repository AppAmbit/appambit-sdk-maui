using System;
using System.Runtime.CompilerServices;

namespace AppAmbitMaui;

public static class Crashes
{
    public static Task<bool> DidCrashInLastSession()
    {
        return AppAmbit.Crashes.DidCrashInLastSession();
    }

    public static Task GenerateTestCrash()
    {
        return AppAmbit.Crashes.GenerateTestCrash();
    }

    public static async Task LogError(string message,
            Dictionary<string, string>? properties = null,
            string? classFqn = null,
            Exception? exception = null,
            [CallerFilePath] string? fileName = null,
            [CallerLineNumber] int? lineNumber = null)
    {
        AppAmbit.Crashes.LogError(message, properties, classFqn, exception, fileName, lineNumber);
    }

    public static async Task LogError(Exception exception,
            Dictionary<string, string>? properties = null,
            string? classFqn = null,
            [CallerFilePath] string? fileName = null,
            [CallerLineNumber] int lineNumber = 0)
    {
        AppAmbit.Crashes.LogError(exception, properties, classFqn, fileName, lineNumber);
    }

}