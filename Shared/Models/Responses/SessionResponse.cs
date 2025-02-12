using Newtonsoft.Json;

namespace Shared.Models.Responses;

public class SessionResponse
{
    [JsonProperty("session_id")]
    public string SessionId { get; set; }
}