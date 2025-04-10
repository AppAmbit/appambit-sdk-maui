using Newtonsoft.Json;
using System.Collections.Generic;

namespace AppAmbit.Models.Analytics;

public class Event
{
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("metadata")]
    public Dictionary<string,string> Data { get; set; } = new Dictionary<string,string>();
}