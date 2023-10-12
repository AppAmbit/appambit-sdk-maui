using System.Net;
using System.Net.Http.Headers;

namespace HttpClientWow.Tokens;

public class TokenRetrievalHandler : DelegatingHandler
{
	private readonly ITokenService _tokenService;

	public TokenRetrievalHandler(ITokenService tokenService)
	{
		_tokenService = tokenService;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		request.Options.TryGetValue(new HttpRequestOptionsKey<bool>("isAuthEndpoint"), out var isAuthEndpoint);

		if (!isAuthEndpoint)
		{
			return await base.SendAsync(request, cancellationToken);
		}
		
		var token = _tokenService.GetToken(Constants.Tb12AccessTokenName) 
		            ?? await _tokenService.RefreshAccessToken();
		
		if (token?.AccessToken is null) throw new Exception("Could not get or refresh token");
		
		request.Headers.Authorization = new AuthenticationHeaderValue(token.Scheme, token.AccessToken);

		var response = await base.SendAsync(request, cancellationToken);

		if (response.StatusCode == HttpStatusCode.Unauthorized)
		{
			throw new Exception("Token is outdated");
		}

		return response;
	}
}