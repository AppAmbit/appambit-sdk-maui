using Newtonsoft.Json;

namespace AppAmbit.Models.Analytics;

public class 
    
    AnalyticsReport
{
    [JsonProperty("event_title")]
    public string EventTitle { get; set; }
    
    [JsonProperty("session_id")]
    public string SessionId { get; set; }
    
    [JsonProperty("data")]
    public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
}