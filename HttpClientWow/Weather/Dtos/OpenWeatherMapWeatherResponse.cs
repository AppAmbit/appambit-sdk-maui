using System.Text.Json.Serialization;

namespace HttpClientWow.Weather.Dtos;

public record OpenWeatherMapWeatherResponse
{
	[JsonPropertyName("main")] public Main main { get; set; }

	public record Main
	{
		[JsonPropertyName("temp")] public float Temp { get; set; }
		[JsonPropertyName("feels_like")] public float FeelsLike { get; set; }
	}
};