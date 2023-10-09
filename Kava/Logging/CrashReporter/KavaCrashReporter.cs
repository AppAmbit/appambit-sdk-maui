namespace Kava.Logging.CrashReporter;

public class KavaCrashReporter
{
    private static KavaCrashLogger _crashLogger;
    
    public void Init(KavaCrashLogger crashLogger)
    {
        _crashLogger = crashLogger;
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