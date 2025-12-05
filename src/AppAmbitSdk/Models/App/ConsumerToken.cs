using System;
using Newtonsoft.Json;

namespace AppAmbitSdkCore.Models.App;

internal class ConsumerToken
{
    [JsonProperty("app_key")]
    public string appKey { get; set; } = string.Empty;

    [JsonProperty("consumer_id")]
    public string consumerId { get; set; } = string.Empty;
}
