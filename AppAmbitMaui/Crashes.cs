using System.Diagnostics;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit;

public static class Crashes
{
    static Crashes()
    { 
        AppDomain.CurrentDomain.UnhandledException -= Logging.OnUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += Logging.OnUnhandledException;
    }
    public static async Task LogError(Exception? ex, Dictionary<string, string> properties = null)
    {
        var tempDict = new Dictionary<string, string>();
        if (properties != null)
        {
            foreach (var item in properties.TakeWhile(item => tempDict.Count <= 80))
            {
                tempDict.Add(item.Key, Truncate(item.Value, 125));
            }
        }
        
        await LogEvent( ex?.StackTrace, LogType.Error, ex, tempDict);
    }
    
    public static async Task LogError(string message)
    {
        await LogEvent(message, LogType.Error);
    }

    public static async Task GenerateTestCrash()
    {
        await LogEvent( "This is a test crash", LogType.Crash);
    }

    private static async Task LogEvent(string? message, LogType logType, Exception? exception = null,
        Dictionary<string,string> properties = null)
    {
        var propertiesDict = properties?.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        await Logging.LogEvent(message, LogType.Error,exception, propertiesDict);
    }
    
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }
}