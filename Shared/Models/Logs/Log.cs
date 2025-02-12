using SQLite;

namespace Shared.Models.Logs;

public class Log
{
    [PrimaryKey]
    public Guid Id { get; set; }
        
    public string? Title { get; set; }
    
    public string? Message { get; set; }
    
    public LogType Type { get; set; }
    
    public string? AppVersion { get; set; }
    
    public DateTime Timestamp { get; set; }
}

public enum LogType
{
    Debug,
    Information,
    Warning,
    Error,
    Crash
}