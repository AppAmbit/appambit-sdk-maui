using AppAmbit.Utils;
using Newtonsoft.Json;

namespace AppAmbit.Models.Analytics;

public class SessionData : IIdentifiable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
}
