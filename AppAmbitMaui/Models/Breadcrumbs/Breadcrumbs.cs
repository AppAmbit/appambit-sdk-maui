
using Newtonsoft.Json;

namespace AppAmbit.Models.Breadcrums;

public class Breadcrumb
{
    [JsonProperty("breadcrumb_name")]
    public string Name { get; set; }
}
