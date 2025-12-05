using Newtonsoft.Json;

namespace AppAmbitSdkCore.Models.Responses;

public class SessionResponse
{
    [JsonProperty("session_id")]
    public string SessionId { get; set; }
}