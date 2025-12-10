#if IOS
using System.Runtime.InteropServices;
using System.Text;
using AppAmbit;
using UIKit;
#endif

#if ANDROID
using Java.Lang;
using StringBuilder = System.Text.StringBuilder;
using Android.OS;
#endif
namespace AppAmbitTestingApp.Models;
#if IOS
internal partial class CrashFileGeneratorIOSApp
{
    [DllImport("libc")]
    private static extern IntPtr pthread_self();

    [DllImport("libc")]
    private static extern int pthread_getname_np(IntPtr thread, IntPtr name, ulong len);
    
    [DllImport("libSystem")]
    private static extern IntPtr mach_task_self();
    
    [DllImport("libSystem")]
    private static extern int task_threads(IntPtr target_task, out IntPtr thread_list, out int thread_count);
    
    [DllImport("libc")]
    private static extern int backtrace(IntPtr[] buffer, int size);
    
    [DllImport("libc")]
    private static extern IntPtr backtrace_symbols(IntPtr[] buffer, int size);

    public static void AddHeader(StringBuilder log, string deviceId)
    {
        log.AppendLine($"Package: {AppInfo.PackageName}");
        log.AppendLine($"Version Code: {AppInfo.BuildString}");
        log.AppendLine($"Version Name: {AppInfo.VersionString}");
        log.AppendLine($"iOS: {UIDevice.CurrentDevice.SystemVersion}");
        log.AppendLine($"Manufacturer: Apple");
        log.AppendLine($"Model: {DeviceInfo.Model}");
        log.AppendLine($"Device Id: {deviceId}");
        log.AppendLine($"Date: {DateTime.UtcNow:O}");
    }
    
    public static void AddThreads(StringBuilder log)
    {
        IntPtr threadListPtr;
        int threadCount;

        if (task_threads(mach_task_self(), out threadListPtr, out threadCount) != 0)
        {
            log.AppendLine("Failed to get task threads.");
            return;
        }

        IntPtr[] threads = new IntPtr[threadCount];
        Marshal.Copy(threadListPtr, threads, 0, threadCount);

        for (int i = 0; i < threadCount; i++)
        {
            IntPtr machThread = threads[i];
            IntPtr pthread = pthread_self();

            byte[] nameBuffer = new byte[256];
            GCHandle handle = GCHandle.Alloc(nameBuffer, GCHandleType.Pinned);
            IntPtr namePtr = handle.AddrOfPinnedObject();
            int result = pthread_getname_np(pthread, namePtr, (ulong)nameBuffer.Length);
            handle.Free();

            string threadName = Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
            log.AppendLine($"Thread {i}: {threadName}");

            IntPtr[] buffer = new IntPtr[64];
            int frames = backtrace(buffer, buffer.Length);
            IntPtr symbolsPtr = backtrace_symbols(buffer, frames);

            if (symbolsPtr != IntPtr.Zero)
            {
                for (int j = 0; j < frames; j++)
                {
                    IntPtr symbolPtr = Marshal.ReadIntPtr(symbolsPtr, j * IntPtr.Size);
                    string symbol = Marshal.PtrToStringAnsi(symbolPtr) ?? "Unknown";
                    log.AppendLine($"  {symbol}");
                }
            }

            log.AppendLine();
        }
    }
}
#endif


#if ANDROID
internal static class CrashFileGeneratorAndroidApp
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