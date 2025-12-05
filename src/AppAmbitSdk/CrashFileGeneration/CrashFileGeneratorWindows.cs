#if WINDOWS
using System.Diagnostics;
using System.Text;
using System.Reflection;
using Microsoft.Win32;

namespace AppAmbitSdkCore;

internal static class CrashFileGeneratorWindows
{
    public static void AddHeader(StringBuilder log, string deviceId)
    {
        var info = new AppAmbitSdkCore.Services.AppInfoService();

        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var processName = Process.GetCurrentProcess().ProcessName;
        var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);

        var os = Environment.OSVersion;
        var machineName = Environment.MachineName;
        var cpuModel = GetCpuName();

        log.AppendLine($"Process: {processName}");
        log.AppendLine($"Product: {fileVersion.ProductName ?? "Unknown"}");
        log.AppendLine($"Version: {info.AppVersion ?? fileVersion.ProductVersion}");
        log.AppendLine($"Build: {info.Build ?? "Unknown"}");
        log.AppendLine($"Windows Version: {os.VersionString}");
        log.AppendLine($"Machine Name: {machineName}");
        log.AppendLine($"CPU: {cpuModel}");
        log.AppendLine($"Device Id: {deviceId}");
        log.AppendLine($"Date: {DateTime.UtcNow:O}");
        log.AppendLine();
    }

    public static void AddThreads(StringBuilder log)
    {
        var currentProcess = Process.GetCurrentProcess();
        var threads = currentProcess.Threads.Cast<ProcessThread>().OrderBy(t => t.Id);

        foreach (var thread in threads)
        {
            log.AppendLine($"Thread {thread.Id} - State: {thread.ThreadState}");

            try
            {
                if (thread.Id == Thread.CurrentThread.ManagedThreadId)
                {
                    var trace = new StackTrace(true);
                    int count = 0;
                    foreach (var frame in trace.GetFrames() ?? Array.Empty<StackFrame>())
                    {
                        var method = frame.GetMethod();
                        var countPadded = ("" + count++).PadRight(4);
                        log.AppendLine($"{countPadded}    at {method?.DeclaringType?.FullName}.{method?.Name} ({frame.GetFileName()}:{frame.GetFileLineNumber()})");
                    }
                }
                else
                {
                    log.AppendLine("    Stack trace not available for external threads.");
                }
            }
            catch (Exception ex)
            {
                log.AppendLine($"    Error while retrieving stack trace: {ex.Message}");
            }

            log.AppendLine();
        }
    }

    private static string GetCpuName()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
            var name = key?.GetValue("ProcessorNameString")?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(name))
                return name;

            var envName = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            return !string.IsNullOrEmpty(envName) ? envName : "Unknown CPU";
        }
        catch
        {
            return "Unknown CPU";
        }
    }
}
#endif