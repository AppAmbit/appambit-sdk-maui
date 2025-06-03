using System;
using Newtonsoft.Json;

namespace AppAmbit.Models.Analytics;

public class SessionsPayload
{
    [JsonProperty("sessions")]
    public List<SessionBatch> Sessions { get; set; }
}

public class SessionBatch
{
    [JsonProperty("started_at")]
    public DateTime? StartedAt { get; set; }

    [JsonProperty("ended_at")]
    public DateTime? EndedAt { get; set; }
}