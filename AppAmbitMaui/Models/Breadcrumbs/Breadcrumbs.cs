
using Newtonsoft.Json;

namespace AppAmbit.Models.Breadcrums;

public class Breadcrumb
{
    [JsonProperty("name")]
    public string Name { get; set; }
}
