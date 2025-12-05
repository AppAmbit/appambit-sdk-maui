using Newtonsoft.Json;

namespace AppAmbitSdkCore.Models.Logs;

public class LogResponse
{
    [JsonProperty("id")]
    public int Id { get; set; }
    
    [JsonProperty("hash")]
    public string Hash { get; set; }
    
    [JsonProperty("consumers")]
    public int Consumers { get; set; }
    
    [JsonProperty("occurrences")]
    public int Occurrences { get; set; }
    
    [JsonProperty("classFQN")]
    public string ClassFQN { get; set; }
    
    [JsonProperty("message")]
    public string Message { get; set; }
}