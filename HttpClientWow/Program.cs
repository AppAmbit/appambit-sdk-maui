using HttpClientWow;
using HttpClientWow.Handlers;
using HttpClientWow.Tb12;
using HttpClientWow.Tokens;
using HttpClientWow.Weather;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddScoped<CachedWeatherHandler>();

// Adds a named client called "weather" which is transient in scope
// but the HttpClientHandler behind the scenes that actually performs
// the API calls is actually pooled and managed by the HttpClientFactory.
// The client can be init'd and disposed, but the underlying handler can be
// reused
builder.Services
	.AddHttpClient("weather", client =>
	{
		client.BaseAddress = new Uri("https://api.openweathermap.org/data/2.5");
	})
	.AddHttpMessageHandler<CachedWeatherHandler>();

builder.Services.AddSingleton<IWeatherService, OpenWeatherMapService>();

// TB12 STUFF HERE*************************************************************

builder.Services.AddTb12HttpPipeline();

// Set Endpoints and Middleware ***********************************************
var app = builder.Build();

app.UseHttpsRedirection();

app.MapGet("/weather", async (string city, IWeatherService weatherService) =>
	{
		var weather = await weatherService.GetWeatherForCityAsync(city);
		return weather is null
			? Results.NotFound()
			: Results.Ok(weather);
	})
	.WithName("GetWeatherForecast");

app.MapGet("/tb12-login", async ([FromServices]ITb12Service tb12Service) =>
	{
		var loginResponse = await tb12Service.Login();
		return loginResponse is null
			? Results.Unauthorized()
			: Results.Ok(loginResponse);
	})
	.WithName("Tb12-login");

app.MapGet("/tb12-get-user", async ([FromServices]ITb12Service tb12Service) =>
	{
		var userResponse = await tb12Service.UserInfo();
		return userResponse is null
			? Results.Unauthorized()
			: Results.Ok(userResponse);
	})
	.WithName("Tb12-get-user");

app.MapGet("/tb12-invalidate-token", ([FromServices]ITb12Service tb12Service) =>
{
	tb12Service.InvalidateAccessToken();
	return Results.Ok("Tb12 Access Token has been invalidated");
});

app.Run();