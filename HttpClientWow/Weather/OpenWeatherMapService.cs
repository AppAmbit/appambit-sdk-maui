using System.Net;
using HttpClientWow.Weather.Dtos;

namespace HttpClientWow.Weather;

public class OpenWeatherMapService : IWeatherService
{
	private const string _apiKey = "c878a707f82e5415f724f8767757559e";
	private readonly HttpClient _client;

	public OpenWeatherMapService(IHttpClientFactory httpClientFactory)
	{
		_client = httpClientFactory.CreateClient("weather");
	}

	public async Task<WeatherResponse?> GetWeatherForCityAsync(string city)
	{
		var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}";

		var weatherResponse = await _client.GetAsync($"/weather?q={city}&appid={_apiKey}");
		if (weatherResponse.StatusCode == HttpStatusCode.NotFound) return null;

		var weather = await weatherResponse.Content.ReadFromJsonAsync<OpenWeatherMapWeatherResponse>();

		return new WeatherResponse(weather!.main.Temp, weather.main.FeelsLike);
	}
}