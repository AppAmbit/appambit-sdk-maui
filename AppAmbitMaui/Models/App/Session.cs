using Newtonsoft.Json;

namespace AppAmbit.Models.App;

public class Session
{
    [JsonProperty("timestamp")]
    public string Timestamp { get; set; }
    
    [JsonProperty("session_id")]
    public string SessionId { get; set; }
}