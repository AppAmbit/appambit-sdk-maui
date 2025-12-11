using Newtonsoft.Json;

namespace AppAmbit.Models.App;

internal class UpdateConsumer
{
    public UpdateConsumer(string deviceToken, bool pushEnabled)
    {
        DeviceToken = deviceToken;
        PushEnabled = pushEnabled;
    }

    [JsonProperty("device_token")]
    public string DeviceToken { get; }

    [JsonProperty("push_enabled")]
    public bool PushEnabled { get; }
}
