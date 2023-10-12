using System.Text.Json.Serialization;

namespace HttpClientWow.Tb12.Dtos;

public record RefreshTokenResponse(
	[property: JsonPropertyName("token")] string AccessToken,
	[property: JsonPropertyName("refreshToken")] string RefreshToken
);