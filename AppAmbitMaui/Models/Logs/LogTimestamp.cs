using Newtonsoft.Json;
using SQLite;

namespace AppAmbit.Models.Logs;

//This class is created for storing on the database and send it in the bulk upload
internal class LogTimestamp : Log
{
    [PrimaryKey]
    public Guid Id { get; set; }
    
    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; }
}