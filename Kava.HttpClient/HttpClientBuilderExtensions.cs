using Kava.HttpClient.Authentication;
using Kava.HttpClient.Authentication.Tb12;
using Kava.HttpClient.Authentication.Tokens;
using Microsoft.Extensions.DependencyInjection;

namespace Kava.HttpClient;

public static class HttpClientBuilderExtensions
{
	public static IServiceCollection AddTb12HttpPipeline(this IServiceCollection services)
	{
		
		services.AddScoped<ITokenService, TokenService>();
		services.AddSingleton<ITb12Service, Tb12Service>();
		services.AddScoped<TokenRetrievalHandler>();

		services
			.AddHttpClient(Constants.Tb12ClientName, client =>
			{
				client.BaseAddress = new Uri("https://staging-api-tb12.kavaup.io/api/");
			}) 
			.AddHttpMessageHandler<TokenRetrievalHandler>();
		
		return services;
	}
}