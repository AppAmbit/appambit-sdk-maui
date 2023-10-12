using HttpClientWow.Tb12;
using HttpClientWow.Tokens;

namespace HttpClientWow;

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