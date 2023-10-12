using System.Net.Http.Json;
using Kava.HttpClient.Authentication.Tb12.Dtos;
using Microsoft.Extensions.Caching.Memory;

namespace Kava.HttpClient.Authentication.Tokens;

public class TokenService : ITokenService
{
	private readonly IMemoryCache _memoryCache;
	private readonly System.Net.Http.HttpClient _httpClient;

	public TokenService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
		_httpClient = httpClientFactory.CreateClient();
		_httpClient.BaseAddress = new Uri("https://staging-api-tb12.kavaup.io/api/");
		_httpClient.DefaultRequestHeaders.Accept.Add(new("application/json"));
	}

	public Token? GetToken(string name)
	{
		var token = _memoryCache.Get<Token>(name);
		return token;
	}

	public void StoreToken(string name, Token token)
	{
		_memoryCache.Set(name, token);
	}

	public async Task<Token?> RefreshAccessToken()
	{
		var currentRefreshToken = GetToken(Constants.Tb12RefreshTokenName);
		if (currentRefreshToken is null) throw new Exception("Refresh token is not set");

		var request = new HttpRequestMessage(HttpMethod.Post, "https://staging-api-tb12.kavaup.io/api/refresh-tokens");
		request.Headers.Accept.Add(new("application/json"));
		request.Content = JsonContent.Create(new {refreshToken = currentRefreshToken.AccessToken});

		var response = await _httpClient.SendAsync(request);
		
		var tokenResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
		if (tokenResponse is null) return null;
		
		var accessToken = new Token(Constants.Tb12AccessTokenName, "Bearer", tokenResponse.AccessToken);
		var refreshToken = new Token(Constants.Tb12RefreshTokenName, "", tokenResponse.RefreshToken);
		_memoryCache.Set(Constants.Tb12AccessTokenName, accessToken);
		_memoryCache.Set(Constants.Tb12RefreshTokenName, refreshToken);
		
		return accessToken;
	}

	public void InvalidateToken(string name)
	{
		if (_memoryCache.TryGetValue(name, out _))
		{
			_memoryCache.Remove(name);
		}
	}
}

public interface ITokenService
{
	Token? GetToken(string name);
	void StoreToken(string name, Token value);
	Task<Token?> RefreshAccessToken();
	void InvalidateToken(string name);
}
