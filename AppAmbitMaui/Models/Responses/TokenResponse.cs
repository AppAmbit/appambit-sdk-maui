using Newtonsoft.Json;

namespace AppAmbit.Models.Responses;

public class TokenResponse
{
    [JsonProperty("id")]
    public string ConsumerId {get; set;}
    
    [JsonProperty("token")]
    public string Token {get; set;}
}