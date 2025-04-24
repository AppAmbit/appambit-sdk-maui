using Newtonsoft.Json;

namespace AppAmbit.Models.App;

public class Consumer
{
    [JsonProperty("app_key")] public string AppKey { get; set; }

    [JsonProperty("device_id")] public string DeviceId { get; set; }

    [JsonProperty("device_model")] public string DeviceModel { get; set; }

    [JsonProperty("user_id")] public string UserId { get; set; }

    [JsonProperty("user_email")] public string UserEmail { get; set; }

    [JsonProperty("os")] public string OS { get; set; }

    [JsonProperty("country")] public string Country { get; set; }

    [JsonProperty("language")] public string Language { get; set; }
}