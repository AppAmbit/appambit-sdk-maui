
using Newtonsoft.Json;

namespace AppAmbit.Models.Breadcrubms;

public class Breadcrumb
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
