using Newtonsoft.Json;

namespace Shared.Models.App;

public class Timestamp
{
    [JsonProperty("timestamp")]
    public DateTime Time { get; set; }
}