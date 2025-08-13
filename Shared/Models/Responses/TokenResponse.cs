using Newtonsoft.Json;

namespace Shared.Models.Responses;

public class TokenResponse
{
    [JsonProperty("token")]
    public string Token { get; set; }
    
    [JsonProperty("id")]
    public string ConsumerId { get; set; }
}