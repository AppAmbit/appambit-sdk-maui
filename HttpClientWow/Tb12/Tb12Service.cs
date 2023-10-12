using System.Net.Http.Headers;
using System.Text;
using HttpClientWow.Tb12.Dtos;
using HttpClientWow.Tokens;

namespace HttpClientWow.Tb12;

public class Tb12Service : ITb12Service
{
	private readonly HttpClient _client;
	private readonly ITokenService _tokenService;

	public Tb12Service(IHttpClientFactory httpClientFactory, ITokenService tokenService)
	{
		_tokenService = tokenService;
		_client = httpClientFactory.CreateClient(Constants.Tb12ClientName);
	}

	public async Task<LoginRoot?> Login()
	{
		var request = new HttpRequestMessage(HttpMethod.Post, new Uri("https://staging-api-tb12.kavaup.io/api/users/login/"));
		request.Options.Set(new HttpRequestOptionsKey<bool>("isAuthEndpoint"), false);
		request.Content = new StringContent(
			"{\"email\": \"admin@admin.com\", \"password\": \"password1\"}",
			Encoding.UTF8,
			new MediaTypeHeaderValue("application/json"));

		var response = await _client.SendAsync(request);
			
		var loginResponse = await response.Content.ReadFromJsonAsync<LoginRoot>();
		if (loginResponse is null) return null;
		
		var accessToken = new Token(Constants.Tb12AccessTokenName, "Bearer", loginResponse.Token);
		var refreshToken = new Token(Constants.Tb12RefreshTokenName, "", loginResponse.RefreshToken);
		_tokenService.StoreToken(Constants.Tb12AccessTokenName, accessToken);
		_tokenService.StoreToken(Constants.Tb12RefreshTokenName, refreshToken);

		return loginResponse;
	}

	public async Task<UserInfo?> UserInfo()
	{
		var request = new HttpRequestMessage(HttpMethod.Get, new Uri("https://staging-api-tb12.kavaup.io/api/users/me/"));
		request.Options.Set(new HttpRequestOptionsKey<bool>("isAuthEndpoint"), true);
		
		var response = await _client.SendAsync(request);
		var userInfoResponse = await response.Content.ReadFromJsonAsync<UserInfo>();
		return userInfoResponse;
	}

	public void InvalidateAccessToken()
	{
		_tokenService.InvalidateToken(Constants.Tb12AccessTokenName);
	}
}

public interface ITb12Service
{
	Task<LoginRoot?> Login();
	Task<UserInfo?> UserInfo();
	void InvalidateAccessToken();
}
