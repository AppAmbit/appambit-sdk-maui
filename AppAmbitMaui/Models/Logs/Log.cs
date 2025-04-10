using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using SQLite;

namespace AppAmbit.Models.Logs;

internal class Log
{
    [JsonProperty("app_version")]
    public string? AppVersion { get; set; }
    
    [JsonProperty("classFQN")]
    public string? ClassFQN { get; set; }
    
    [JsonProperty("file_name")]
    public string? FileName { get; set; }

    [JsonProperty("line_number")] 
    public long LineNumber { get; set; }
    
    [JsonProperty("message")]
    public string? Message { get; set; } = String.Empty;

    [JsonProperty("stack_trace")] 
    public string? StackTrace { get; set; } = AppConstants.NoStackTraceAvailable;

    [Ignore] // SQLite ignore this field, it does not support Dictionary Types
    [JsonProperty("context")]
    public Dictionary<string,string> Context 
    { 
        get => string.IsNullOrEmpty(ContextJson) ? new Dictionary<string,string>() : JsonConvert.DeserializeObject<Dictionary<string,string>>(ContextJson);
        set => ContextJson = JsonConvert.SerializeObject(value);
    }

    // internal field for storing on Sqlite
    [JsonIgnore]
    public string ContextJson { get; set; } = "{}";
    
    [JsonProperty("type")]
    public LogType? Type { get; set; }
}

[JsonConverter(typeof(StringEnumConverter))]
public enum LogType
{
    [EnumMember(Value = "error")]
    Error,
    
    [EnumMember(Value = "crash")]
    Crash
}
