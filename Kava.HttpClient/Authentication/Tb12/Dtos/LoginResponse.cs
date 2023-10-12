using System.Text.Json.Serialization;

namespace Kava.HttpClient.Authentication.Tb12.Dtos;

public record LoginResponse(
	[property: JsonPropertyName("user")] UserInfo _userInfo,
	[property: JsonPropertyName("token")] string Token,
	[property: JsonPropertyName("refreshToken")] string RefreshToken
);