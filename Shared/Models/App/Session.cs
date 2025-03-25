using Newtonsoft.Json;

namespace Shared.Models.App;

public class Session
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonProperty("session_id")]
    public string SessionId { get; set; }
}