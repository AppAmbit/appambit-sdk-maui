using Newtonsoft.Json;

namespace AppAmbit.Models.Analytics;

public class EndSession
{
    [JsonProperty("session_id")]
    public string Id { get; set; }
    
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
}