using Newtonsoft.Json;

namespace AppAmbitSdkCore.Models.Breadcrumbs;

public class BreadcrumbsPayload
{
    [JsonProperty("breadcrumbs")]
    public List<BreadcrumbData> Breadcrumbs { get; set; }
}
