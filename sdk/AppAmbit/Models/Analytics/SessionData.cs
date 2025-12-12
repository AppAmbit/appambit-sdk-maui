using AppAmbit.Enums;
using AppAmbit.Utils;
using Newtonsoft.Json;

namespace AppAmbit.Models.Analytics;

public class SessionData : IIdentifiable
{
    /// <summary>
    /// Unique identifier for the session data.
    /// </summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary>
    /// Unique identifier for the end session.
    /// </summary>
    [JsonProperty("session_id")]
    public string? SessionId { get; set; }

    /// <summary>
    /// Timestamp of the session data, indicating when the session started or ended.
    /// </summary>
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Session type, indicating whether it is session end or start to identify the timestamps.
    /// </summary>
    [JsonProperty("session_type")]
    public SessionType? SessionType { get; set; }
}
