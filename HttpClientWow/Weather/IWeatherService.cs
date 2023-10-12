using HttpClientWow.Weather.Dtos;

namespace HttpClientWow.Weather;

public interface IWeatherService
{
	Task<WeatherResponse?> GetWeatherForCityAsync(string city);
}