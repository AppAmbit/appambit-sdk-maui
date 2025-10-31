using AppAmbit.Utils;
using Newtonsoft.Json;
using SQLite;

namespace AppAmbit.Models.Breadcrums;

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
