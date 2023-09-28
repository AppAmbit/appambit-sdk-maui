namespace Kava.Logging.CrashReporter;

public class KavaCrashReporter
{
    private static KavaCrashLogger _crashLogger = new KavaCrashLogger();

    public static void Init()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }
    
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        Task.Run(async () =>
        {
            await _crashLogger.EnterCrashLog(unhandledExceptionEventArgs);
        });
    }
}