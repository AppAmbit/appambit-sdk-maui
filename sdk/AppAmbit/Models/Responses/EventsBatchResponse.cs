using Newtonsoft.Json;

namespace AppAmbit.Models.Responses;

public class EventsBatchResponse
{
    [JsonProperty("message")]
    public string Message { get; set; }
    
    [JsonProperty("status")]
    public string Status { get; set; }
    
    [JsonProperty("jobUuid")]
    public string JobUUID { get; set; }
}