using System;
using Newtonsoft.Json;

namespace KavaupMaui.Models
{
	public class TestResponse
	{
        [JsonProperty("data")]
        public string Data { get; set; }
    }
}

