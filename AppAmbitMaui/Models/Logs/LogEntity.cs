using Newtonsoft.Json;
using SQLite;

namespace AppAmbit.Models.Logs;

//This class is created for storing on the database and send it in the bulk upload
internal class LogEntity : Log
{
    [PrimaryKey]
    [JsonIgnore]
    public Guid Id { get; set; }

    [JsonProperty("session_id")]
    public string? SessionId { get; set; }
    
    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }
}