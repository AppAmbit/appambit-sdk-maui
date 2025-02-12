using Newtonsoft.Json;

namespace Shared.Models.App;

public class Consumer
{
    [JsonProperty("app_version")]
    public string AppVersion { get; set; }

    [JsonProperty("device_id")]
    public string DeviceId { get; set; }

    [JsonProperty("user_id")]
    public string UserId { get; set; }

    [JsonProperty("is_guest")]
    public bool IsGuest { get; set; }

    [JsonProperty("user_email")]
    public string UserEmail { get; set; }

    [JsonProperty("os")]
    public string OS { get; set; }

    [JsonProperty("platform")]
    public string Platform { get; set; }

    [JsonProperty("device_model")]
    public string DeviceModel { get; set; }

    [JsonProperty("country")]
    public string Country { get; set; }
    
    [JsonProperty("language")]
    public string Language { get; set; }
    
    [JsonProperty("app_key")]
    public string AppKey { get; set; }
}