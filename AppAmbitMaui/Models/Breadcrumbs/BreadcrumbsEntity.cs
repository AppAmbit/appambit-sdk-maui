using Newtonsoft.Json;
using SQLite;

namespace AppAmbit.Models.Breadcrums;

public class BreadcrumEntity : Breadcrumb
{
    [PrimaryKey]
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }
}
