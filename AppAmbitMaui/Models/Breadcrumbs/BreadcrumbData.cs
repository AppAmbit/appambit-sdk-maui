using AppAmbit.Utils;
using Newtonsoft.Json;

namespace AppAmbit.Models.Breadcrumbs;

public class BreadcrumbData: IIdentifiable
{
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("session_id")]
    public string? SessionId { get; set; }

    [JsonProperty("created_at")]
    public DateTime Timestamp { get; set; }

    [JsonProperty("name")]
    public string? Name { get; set; }
}
