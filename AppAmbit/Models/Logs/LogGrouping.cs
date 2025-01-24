using Newtonsoft.Json;

namespace AppAmbit.Models.Logs;

public class LogGrouping
{
    [JsonProperty("title")]
    public string? Title { get; set; }
    
    [JsonProperty("description")]
    public string? Description { get; set; }
    
    [JsonProperty("log_type")]
    public string LogType { get; set; }
    
    [JsonProperty("count")]
    public int Count { get; set; }
}