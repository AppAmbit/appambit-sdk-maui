#if IOS
using System.Runtime.InteropServices;
using System.Text;
using AppAmbitSdkCore.Services;
using UIKit;

namespace AppAmbitSdkCore;

internal partial class CrashFileGeneratorIOS
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
        var info = new AppInfoService();
        var bundleId = Foundation.NSBundle.MainBundle?.BundleIdentifier;
        log.AppendLine($"Package: {bundleId}");
        log.AppendLine($"Version Code: {info.Build}");
        log.AppendLine($"Version Name: {info.AppVersion}");
        log.AppendLine($"iOS: {UIDevice.CurrentDevice.SystemVersion}");
        log.AppendLine($"Manufacturer: Apple");
        log.AppendLine($"Model: {info.DeviceModel}");
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
