using Newtonsoft.Json;

namespace AppAmbitSdkCore.Models.Responses;

public class Response
{
    [JsonProperty("message")]
    public string Message { get; set; }
}