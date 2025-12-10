using Newtonsoft.Json;

namespace AppAmbit.Models.App;

public class Timestamp
{
    [JsonProperty("timestamp")]
    public DateTime Time { get; set; }
}