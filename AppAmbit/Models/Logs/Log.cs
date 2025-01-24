using SQLite;

namespace AppAmbit.Models.Logs;

internal class Log
{
    [PrimaryKey]
    public Guid Id { get; set; }
        
    public string? Title { get; set; }
    
    public string? Message { get; set; }
    
    public LogType Type { get; set; }
    
    public string? AppVersion { get; set; }
    
    public DateTime Timestamp { get; set; }
}