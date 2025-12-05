using System;
using Newtonsoft.Json;
using SQLite;

namespace AppAmbitSdkCore.Models.Analytics;

public class SessionsPayload
{
    [JsonProperty("sessions")]
    public List<SessionBatch> Sessions { get; set; }
}

[Table("SessionEntity")]
public class SessionBatch
{
    [JsonIgnore]
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