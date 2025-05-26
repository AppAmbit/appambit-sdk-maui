using System;
using Newtonsoft.Json;

namespace AppAmbit.Models.Analytics;

public class StartSession
{
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
}
