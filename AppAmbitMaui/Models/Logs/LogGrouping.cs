using Newtonsoft.Json;

namespace AppAmbit.Models.Logs;

public class LogGrouping
{
    [JsonProperty("title")] public string? Title { get; set; } = "";
    
    [JsonProperty("description")]
    public string? Description { get; set; }
    
    [JsonProperty("stack_trace")]
    public string? StackTrace { get; set; }

    [JsonProperty("properties")]
    public string? Properties { get; set; }
    
    [JsonProperty("log_type")]
    public string LogType { get; set; }
    
    [JsonProperty("app_version_build")]
    public string AppVersionBuild { get; set; }
    
    [JsonProperty("timestamp")]
    public string Timestamp { get; set; }
    
    [JsonProperty("count")]
    public int Count { get; set; }
}