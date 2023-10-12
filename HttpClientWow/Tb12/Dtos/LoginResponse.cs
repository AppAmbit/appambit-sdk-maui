using System.Text.Json.Serialization;

namespace HttpClientWow.Tb12.Dtos;

public record LoginResponse(
	[property: JsonPropertyName("user")] UserInfo _userInfo,
	[property: JsonPropertyName("token")] string Token,
	[property: JsonPropertyName("refreshToken")] string RefreshToken
);