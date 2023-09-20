namespace Kava.Logging.CrashReporter;

public class KavaCrashReporter
{
    public static void Init()
    {
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }
    
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
    {
        Console.Out.WriteLine("Hey");
    }
}