namespace Kava.Helpers;

public class LogHelper
{
    internal static readonly string LOG_FILE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    internal const string LOG_DIRECTORY = "KavaTemp";
    internal const string LOG_FILE_NAME = "Logs.log";
    
    internal static string GetLogFilePath() => Path.Combine(LOG_FILE_PATH, LOG_DIRECTORY, LOG_FILE_NAME);   
    
}