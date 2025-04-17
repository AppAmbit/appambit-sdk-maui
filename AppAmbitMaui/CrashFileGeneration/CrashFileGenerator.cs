using System.Diagnostics;
using Exception = System.Exception;
using Process = System.Diagnostics.Process;
using StringBuilder = System.Text.StringBuilder;


using System.Runtime.InteropServices;

namespace AppAmbit;

internal static class CrashFileGenerator
{
    public static string GenerateCrashLog(Exception ex, string deviceId)
    {
        StringBuilder log = new StringBuilder();
        
        if(ex == null)
            return log.ToString();
        
        // Header
#if ANDROID
        CrashFileGeneratorAndroid.AddHeader(log,deviceId);
#elif IOS
        CrashFileGeneratorIOS.AddHeader(log,deviceId);
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
#else 
        AddThreads(log);
#endif

        return log.ToString();
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