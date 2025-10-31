using Newtonsoft.Json;

namespace AppAmbit.Models.Breadcrumbs;

public class BreadcrumbsPayload
{
    [JsonProperty("breadcrumbs")]
    public List<BreadcrumbData> Breadcrumbs { get; set; }
}
