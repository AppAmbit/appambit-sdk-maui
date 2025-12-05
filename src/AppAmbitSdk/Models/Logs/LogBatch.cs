using Newtonsoft.Json;

namespace AppAmbitSdkCore.Models.Logs;

internal class LogBatch
{
    [JsonProperty("logs")]
    public List<LogEntity> Logs { get; set; }
}