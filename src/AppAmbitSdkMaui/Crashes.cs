using System;
using System.Runtime.CompilerServices;

namespace AppAmbitSdkMaui;

public static class Crashes
{
    public static Task<bool> DidCrashInLastSession()
    {
        return AppAmbitSdkCore.Crashes.DidCrashInLastSession();
    }

    public static Task GenerateTestCrash()
    {
        return AppAmbitSdkCore.Crashes.GenerateTestCrash();
    }

    public static async Task LogError(string message,
            Dictionary<string, string>? properties = null,
            string? classFqn = null,
            Exception? exception = null,
            [CallerFilePath] string? fileName = null,
            [CallerLineNumber] int? lineNumber = null)
    {
        AppAmbitSdkCore.Crashes.LogError(message, properties, classFqn, exception, fileName, lineNumber);
    }

    public static async Task LogError(Exception exception,
            Dictionary<string, string>? properties = null,
            string? classFqn = null,
            [CallerFilePath] string? fileName = null,
            [CallerLineNumber] int lineNumber = 0)
    {
        AppAmbitSdkCore.Crashes.LogError(exception, properties, classFqn, fileName, lineNumber);
    }

}