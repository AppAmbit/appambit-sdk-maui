#if MACCATALYST
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AppAmbitSdkCore;

internal partial class CrashFileGeneratorMacOS
{
    [DllImport("libSystem")]
    private static extern IntPtr mach_task_self();

    [DllImport("libSystem")]
    private static extern int task_threads(IntPtr target_task, out IntPtr thread_list, out int thread_count);

    [DllImport("libc")]
    private static extern IntPtr pthread_self();

    [DllImport("libc")]
    private static extern int pthread_getname_np(IntPtr thread, IntPtr name, ulong len);

    [DllImport("libc")]
    private static extern int backtrace(IntPtr[] buffer, int size);

    [DllImport("libc")]
    private static extern IntPtr backtrace_symbols(IntPtr[] buffer, int size);

    public static void AddThreads(StringBuilder log)
    {
        IntPtr listPtr;
        int count;

        if (task_threads(mach_task_self(), out listPtr, out count) != 0)
        {
            log.AppendLine("Failed to get task threads.");
            return;
        }

        var threads = new IntPtr[count];
        Marshal.Copy(listPtr, threads, 0, count);

        for (int i = 0; i < count; i++)
        {
            IntPtr pthread = pthread_self();

            byte[] nameBuf = new byte[256];
            GCHandle h = GCHandle.Alloc(nameBuf, GCHandleType.Pinned);
            IntPtr namePtr = h.AddrOfPinnedObject();
            _ = pthread_getname_np(pthread, namePtr, (ulong)nameBuf.Length);
            h.Free();

            string threadName = Encoding.UTF8.GetString(nameBuf).TrimEnd('\0');
            log.AppendLine($"Thread {i}: {threadName}");

            IntPtr[] buffer = new IntPtr[64];
            int frames = backtrace(buffer, buffer.Length);
            IntPtr symbolsPtr = backtrace_symbols(buffer, frames);

            if (symbolsPtr != IntPtr.Zero)
            {
                for (int j = 0; j < frames; j++)
                {
                    IntPtr symPtr = Marshal.ReadIntPtr(symbolsPtr, j * IntPtr.Size);
                    string symbol = Marshal.PtrToStringAnsi(symPtr) ?? "Unknown";
                    log.AppendLine($"  {symbol}");
                }
            }

            log.AppendLine();
        }
    }
}
#endif
