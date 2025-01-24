using Newtonsoft.Json;

namespace AppAmbit.Models.Responses;

public class TokenResponse
{
    [JsonProperty("token")]
    public string Token {get; set;}
}