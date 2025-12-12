#if ANDROID
using System.Diagnostics;
using Java.Lang;
using Exception = System.Exception;
using Process = System.Diagnostics.Process;
using StringBuilder = System.Text.StringBuilder;
using Android.OS;
using System.Runtime.InteropServices;
using AppAmbit.Services;

namespace AppAmbit;

internal static class CrashFileGeneratorAndroid
{
    public static void AddHeader(StringBuilder log, string deviceId)
    {    
        var info = new AppInfoService();
        var packageName = global::Android.App.Application.Context?.PackageName;
        log.AppendLine($"Package: {packageName}");
        log.AppendLine($"Version Code: {info.Build}");
        log.AppendLine($"Version Name: {info.AppVersion}");
        log.AppendLine($"Android: {Build.VERSION.SdkInt}");
        log.AppendLine($"Android Build: {Build.Display}");
        log.AppendLine($"Manufacturer: {Build.Manufacturer}");
        log.AppendLine($"Model: {info.DeviceModel ?? Build.Model}");
        log.AppendLine($"Device Id: {deviceId}");
        log.AppendLine($"Date: {DateTime.UtcNow:O}");
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
