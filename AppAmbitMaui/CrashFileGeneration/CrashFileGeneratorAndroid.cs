
#if ANDROID
using Shared.Utils;
using System.Diagnostics;
using Java.Lang;
using Exception = System.Exception;
using Process = System.Diagnostics.Process;
using StringBuilder = System.Text.StringBuilder;
using Android.OS;
using System.Runtime.InteropServices;
namespace AppAmbit;


internal static class CrashFileGeneratorAndroid
{
    public static void AddHeader(StringBuilder log, string deviceId)
    {    
        log.AppendLine($"Package: {AppInfo.PackageName}");
        log.AppendLine($"Version Code: {AppInfo.BuildString}");
        log.AppendLine($"Version Name: {AppInfo.VersionString}");
        log.AppendLine($"Android: {Build.VERSION.SdkInt}");
        log.AppendLine($"Android Build: {Build.Display}");
        log.AppendLine($"Manufacturer: {Build.Manufacturer}");
        log.AppendLine($"Model: {Build.Model}");
        log.AppendLine($"Device Id: {deviceId}");
        log.AppendLine($"Date: {DateUtils.GetUtcNow:O}");
    }
    public static void AddThreads(StringBuilder log)
    {
        var threadSet = Java.Lang.Thread.AllStackTraces;
        var sortedThreads = threadSet.Keys.OrderBy(thread => thread.Id);

        foreach (var thread in sortedThreads)
        {
            StackTraceElement[] stackTrace = threadSet[thread];

            log.AppendLine($"Thread {thread.Id} - {thread.Name} ({thread.GetState()}):");
            var count = 0;
            foreach (var trace in stackTrace)
            {
                var countPadded = (""+count++).PadRight(4);
                log.AppendLine($"{countPadded}    at {trace.ClassName}.{trace.MethodName} ({trace.FileName}:{trace.LineNumber})");
            }
            log.AppendLine();
        }
    }
}
#endif