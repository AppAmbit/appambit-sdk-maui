using AppAmbitSdkCore.Models.Analytics;
using Newtonsoft.Json;

public class EventBatchPayload
{
    [JsonProperty("events")]
    public List<EventEntity> Events { get; set; }
}