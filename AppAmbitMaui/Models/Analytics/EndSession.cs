using Newtonsoft.Json;
using AppAmbit.Utils;
namespace AppAmbit.Models.Analytics;

public class EndSession : IIdentifiable
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
}