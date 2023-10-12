using System.Text.Json.Serialization;

namespace Kava.HttpClient.Authentication.Tb12.Dtos;

public record RefreshTokenResponse(
	[property: JsonPropertyName("token")] string AccessToken,
	[property: JsonPropertyName("refreshToken")] string RefreshToken
);