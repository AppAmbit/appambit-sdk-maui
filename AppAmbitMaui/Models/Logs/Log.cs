using SQLite;

namespace AppAmbit.Models.Logs;

internal class Log
{
    [PrimaryKey]
    public Guid Id { get; set; }
        
    public string? Title { get; set; }
    
    public string? Description { get; set; }
    
    public string? StackTrace { get; set; }

    
    public string? Properties { get; set; }
    
    public LogType Type { get; set; }
    
    public string? AppVersionBuild { get; set; }
    
    public DateTime Timestamp { get; set; }
}