using AppAmbitSdkCore.Models.Breadcrubms;
using AppAmbitSdkCore.Utils;
using Newtonsoft.Json;
using SQLite;

namespace AppAmbitSdkCore.Models.Breadcrumbs;

public class BreadcrumbsEntity : Breadcrumb
{
    [PrimaryKey]
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("session_id")]
    public string SessionId { get; set; } = string.Empty;
}
