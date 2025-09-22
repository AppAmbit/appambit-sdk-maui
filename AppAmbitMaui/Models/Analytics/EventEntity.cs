using AppAmbit.Utils;
using Newtonsoft.Json;
using SQLite;

namespace AppAmbit.Models.Analytics;

public class EventEntity : Event
{
    [JsonIgnore]
    [PrimaryKey]
    public Guid Id { get; set; }

    [Ignore] // SQLite ignore this field, it does not support Dictionary Types
    [JsonProperty("metadata")]
    public Dictionary<string,string> Data 
    { 
        get => string.IsNullOrEmpty(DataJson) ? new Dictionary<string,string>() : JsonConvert.DeserializeObject<Dictionary<string,string>>(DataJson);
        set => DataJson = JsonConvert.SerializeObject(value);
    }

    public string SessionId { get; set; } = string.Empty;

    // internal field for storing on Sqlite
    [JsonIgnore]
    public string DataJson { get; set; } = "{}";
    
    [JsonProperty("created_at")]
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime CreatedAt { get; set; }
}