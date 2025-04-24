using Newtonsoft.Json;

namespace AppAmbit.Models.Logs;

internal class LogBatch
{
    [JsonProperty("logs")] public List<LogEntity> Logs { get; set; }
}