using Newtonsoft.Json;
using SQLite;

namespace AppAmbitTestingApp.Models;


[Table("SessionEntity")]
public class SessionModel
{
    [PrimaryKey]
    [JsonProperty("id")]
    public string? Id { get; set; }

    [JsonProperty("session_id")]
    public string? SessionId { get; set; }

    [JsonProperty("started_at")]
    public DateTime? StartedAt { get; set; }

    [JsonProperty("ended_at")]
    public DateTime? EndedAt { get; set; }
}
