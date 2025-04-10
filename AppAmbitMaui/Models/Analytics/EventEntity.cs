using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SQLite;

namespace AppAmbit.Models.Analytics;

public class EventEntity : Event
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Ignore] // SQLite ignore this field, it does not support Dictionary Types
    [JsonProperty("context")]
    public Dictionary<string,string> Data 
    { 
        get => string.IsNullOrEmpty(DataJson) ? new Dictionary<string,string>() : JsonConvert.DeserializeObject<Dictionary<string,string>>(DataJson);
        set => DataJson = JsonConvert.SerializeObject(value);
    }

    // internal field for storing on Sqlite
    [JsonIgnore]
    public string DataJson { get; set; } = "{}";
}