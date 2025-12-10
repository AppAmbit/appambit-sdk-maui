using Newtonsoft.Json;

namespace AppAmbit.Models.Responses;

public class SessionResponse
{
    [JsonProperty("session_id")]
    public string SessionId { get; set; }
}