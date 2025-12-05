using Newtonsoft.Json;

namespace AppAmbitSdkCore.Models.App;

public class Timestamp
{
    [JsonProperty("timestamp")]
    public DateTime Time { get; set; }
}