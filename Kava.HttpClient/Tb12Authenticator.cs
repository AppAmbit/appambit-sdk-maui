using System.Text.Json.Serialization;
using RestSharp;
using RestSharp.Authenticators;

namespace Kava.HttpClient;

public class Tb12Authenticator : AuthenticatorBase
{
	public readonly string BaseUrl;
	public readonly string Email;
	public readonly string Password;
	
	public Tb12Authenticator(string baseUrl, string email, string password) 
		: base("")
	{
		BaseUrl = baseUrl;
		Email = email;
		Password = password;
	}
	
	protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
	{
		if (string.IsNullOrEmpty(Token))
		{
			var (token, refreshToken) = await GetToken();
			Token = token;
		}
		
		return new HeaderParameter(KnownHeaders.Authorization, $"Bearer {Token}");
	}

	private async Task<(string token, string refreshToken)> GetToken()
	{
		var options = new RestClientOptions(BaseUrl);
		
		using var client = new RestClient(options);

		var request = new RestRequest("api/users/login")
			.AddJsonBody(new { email = Email, password = Password });

		var response = await client.PostAsync<Tb12PostLoginResponse>(request);
		return (response!.Token, response!.RefreshToken);
	}
}

public record Tb12PostLoginResponse(
	[property: JsonPropertyName("user")] object User,
	[property: JsonPropertyName("token")] string Token,
	[property: JsonPropertyName("refreshToken")] string RefreshToken
);

public record Height(
	[property: JsonPropertyName("feet")] int Feet,
	[property: JsonPropertyName("inches")] int Inches
);

public record Tb12GetAuthUser(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("firstName")] string FirstName,
	[property: JsonPropertyName("lastName")] string LastName,
	[property: JsonPropertyName("birthDate")] object BirthDate,
	[property: JsonPropertyName("age")] int Age,
	[property: JsonPropertyName("height")] Height Height,
	[property: JsonPropertyName("weight")] int Weight,
	[property: JsonPropertyName("focus")] string Focus,
	[property: JsonPropertyName("email")] string Email
);
