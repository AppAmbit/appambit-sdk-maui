using Newtonsoft.Json;
using AppAmbit.Utils;
namespace AppAmbit.Models.Analytics;

public class StartSession : IIdentifiable
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }

}
