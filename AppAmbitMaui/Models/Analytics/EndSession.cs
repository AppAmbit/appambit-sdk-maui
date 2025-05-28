using Newtonsoft.Json;
using AppAmbit.Utils;
namespace AppAmbit.Models.Analytics;

public class EndSession
{
    [JsonProperty("session_id")]
    public string SessionId { get; set; } = string.Empty;

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }    
}