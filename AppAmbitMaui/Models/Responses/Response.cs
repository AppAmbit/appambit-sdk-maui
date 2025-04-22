using Newtonsoft.Json;

namespace AppAmbit.Models.Responses;

public class Response
{
    [JsonProperty("message")]
    public string Message { get; set; }
}