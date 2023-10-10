namespace Kava.Helpers;

public class LogHelper
{
    internal static readonly string LogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    internal const string LogDirectory = "KavaTemp";
    
    static readonly DeviceHelper DeviceHelper = new DeviceHelper();
    internal static readonly string LogFileName = $"Logs_{DeviceHelper.GetDeviceId()}.log";
    
    internal static string GetLogFilePath() => Path.Combine(LogFilePath, LogDirectory, LogFileName);   
    
}