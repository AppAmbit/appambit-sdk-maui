using Newtonsoft.Json;
using SQLite;

namespace AppAmbit.Models.Logs;

public class Log
{
    [PrimaryKey]
    public Guid Id { get; set; }
    
    [JsonProperty("app_version")]
    public string? AppVersion { get; set; }
    
    [JsonProperty("classFQN")]
    public string? ClassFQN { get; set; }
    
    [JsonProperty("file_name")]
    public string? FileName { get; set; }
    
    [JsonProperty("line_number")]
    public long? LineNumber { get; set; }
    
    [JsonProperty("message")]
    public string? Message { get; set; }
    
    [JsonProperty("stack_trace")]
    public string? StackTrace { get; set; }
    
    
    [Ignore] // SQLite ignore this field, it does not support Dictionary Types
    [JsonProperty("context")]
    public Dictionary<string, object> Context 
    { 
        get => string.IsNullOrEmpty(ContextJson) ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(ContextJson);
        set => ContextJson = JsonConvert.SerializeObject(value);
    }

    // internat field for storing on Sqlite
    [JsonIgnore]
    public string ContextJson { get; set; }
    
    [JsonProperty("type")]
    public string? Type { get; set; }
}