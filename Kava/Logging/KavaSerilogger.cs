using System.Globalization;
using System.Text;
using Amazon;
using Kava.Helpers;
using Kava.Logging.Factory;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Kava.Logging;

public class KavaSerilogger : ILogService
{
    private DeviceHelper _deviceHelper = new DeviceHelper();
    private readonly string _logEntryMessage =
        "Log entry created at level: {Level}, tag: {Tag}, message: {Message}, data: {Data}";
    private readonly string _crashEntryMessage =
        "Crash detected! DeviceId: {DeviceId}, ErrorMessage: {ErrorMessage}, Stacktrace: {Stacktrace}";


    public KavaSerilogger()
    {
        SeriloggerFactory.GenerateLogger();
    }
    
    public Task Log(string message, LogLevel level = LogLevel.Information, string tag = ILogService.DEFAULT_TAG) =>
        Task.Run(async () =>
        {
            await LogAsync(message, level, tag);
        });

    public async Task<bool> LogAsync(string message, LogLevel level = LogLevel.Information,
        string tag = ILogService.DEFAULT_TAG)
    {
        return await LogAsync(new LogEntry
        {
            Message = message,
            LogLevel = level,
            LogTag = tag,
            CreatedAt = DateTime.Now
        });
    }

    public Task Log(LogEntry entry) => Task.Run(async () =>
    {
        await LogAsync(entry);
    });

    public async Task<bool> LogAsync(LogEntry entry)
    {
        await Task.Delay(100);
        LogEventLevel eventLevel = GetEventLevel(entry.LogLevel);
        Serilog.Log.Write(eventLevel, _logEntryMessage, entry.LogLevel, entry.LogTag, entry.Message, entry.Data);
        return true;
    }

    public async Task<bool> ClearLogs() => await Task.Run( async () =>
    {
        await Serilog.Log.CloseAndFlushAsync();
        return await FileHelper.ClearLogAsync(LogHelper.GetLogFilePath());
    });
    
    public bool ShouldClearLogs() => false;
    
    private LogEventLevel GetEventLevel(LogLevel level)
    {
        switch (level)
        {
            case LogLevel.Trace:
                return LogEventLevel.Verbose;
            case LogLevel.Debug:
                return LogEventLevel.Debug;
            case LogLevel.Information:
                return LogEventLevel.Information;
            case LogLevel.Warning:
                return LogEventLevel.Warning;
            case LogLevel.Error:
                return LogEventLevel.Error;
            case LogLevel.Critical:
                return LogEventLevel.Fatal;
            default:
                return LogEventLevel.Information;
        }
    }

    public async Task LogCrash(string errorMessage, string stackTrace)
    {
        Serilog.Log.Write(Serilog.Events.LogEventLevel.Fatal, _crashEntryMessage, _deviceHelper.GetDeviceId(), errorMessage, stackTrace);
    }
}