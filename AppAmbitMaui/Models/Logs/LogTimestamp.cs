using Newtonsoft.Json;
using SQLite;

namespace AppAmbit.Models.Logs;

public class LogTimestamp : Log
{
    [PrimaryKey]
    public Guid Id { get; set; }
    
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
}