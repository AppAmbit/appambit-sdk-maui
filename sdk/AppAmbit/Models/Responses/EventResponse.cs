using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AppAmbit.Models.Responses;

public class EventResponse
{
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("consumer_id")]
    public int ConsumerId { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("event_data")]
    public List<EventResponseData> EventData { get; set; } = new List<EventResponseData>();
}

public class EventResponseData
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }

    [JsonProperty("count")]
    public int Count { get; set; }

    [JsonProperty("event_id")]
    public int EventId { get; set; }
}