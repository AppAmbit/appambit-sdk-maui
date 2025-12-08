using Newtonsoft.Json;

namespace AppAmbit.Models.Responses;

public class EndSessionResponse
{
    [JsonProperty("session_id")]
    public string SessionId { get; set; }
    
    [JsonProperty("consumer_id")]
    public string ConsumerId { get; set; }
    
    [JsonProperty("started_at")]
    public string StartedAt { get; set; }
    
    [JsonProperty("end_at")]
    public string EndAt { get; set; }
}