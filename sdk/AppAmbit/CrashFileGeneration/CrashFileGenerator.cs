using System;
using System.Diagnostics;
using Exception = System.Exception;
using Process = System.Diagnostics.Process;
using StringBuilder = System.Text.StringBuilder;
using System.Reflection;
namespace AppAmbit;
internal static class CrashFileGenerator
{
    public static string GenerateCrashLog(Exception ex, string deviceId)
    {
        StringBuilder log = new StringBuilder();
        // Header
#if ANDROID
        CrashFileGeneratorAndroid.AddHeader(log,deviceId);
#elif IOS
        CrashFileGeneratorIOS.AddHeader(log,deviceId);
#elif WINDOWS
        CrashFileGeneratorWindows.AddHeader(log, deviceId);
#else
        if (OperatingSystem.IsWindows())
            AddWindowsHeaderRuntime(log, deviceId);
#endif
        log.AppendLine();
        // Exception Stack Trace
        log.AppendLine("Xamarin Exception Stack:");
        log.AppendLine(ex.StackTrace);
        log.AppendLine();
        // Threads info
#if ANDROID
        CrashFileGeneratorAndroid.AddThreads(log);
#elif IOS
        CrashFileGeneratorIOS.AddThreads(log);
#elif MACCATALYST
        CrashFileGeneratorMacOS.AddThreads(log);
#else
        AddThreads(log);
#endif
        return log.ToString();
    }
    private static void AddWindowsHeaderRuntime(StringBuilder log, string deviceId)
    {
        try
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var processName = Process.GetCurrentProcess().ProcessName;
            var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            log.AppendLine($"Process: {processName}");
            log.AppendLine($"Product: {fileVersion.ProductName ?? "Unknown"}");
            log.AppendLine($"Version: {fileVersion.ProductVersion ?? assembly.GetName().Version?.ToString()}");
            log.AppendLine($"Build: {assembly.GetName().Version?.ToString()}");
            log.AppendLine($"Windows Version: {Environment.OSVersion.VersionString}");
            log.AppendLine($"Machine Name: {Environment.MachineName}");
            log.AppendLine($"Device Id: {deviceId}");
            log.AppendLine($"Date: {DateTime.UtcNow:O}");
            log.AppendLine();
        }
        catch (Exception e)
        {
            log.AppendLine($"Failed to collect Windows header: {e.Message}");
        }
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