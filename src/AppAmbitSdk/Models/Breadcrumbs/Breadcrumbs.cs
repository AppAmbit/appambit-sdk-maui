
using Newtonsoft.Json;

namespace AppAmbitSdkCore.Models.Breadcrubms;

public class Breadcrumb
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
