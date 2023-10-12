using System.Text.Json.Serialization;

namespace Kava.HttpClient.Authentication.Tb12.Dtos;

public record LoginRoot(
	[property: JsonPropertyName("user")] UserInfo _userInfo,
	[property: JsonPropertyName("token")] string Token,
	[property: JsonPropertyName("refreshToken")] string RefreshToken
);

public record UserInfo(
	[property: JsonPropertyName("id")] string Id,
	[property: JsonPropertyName("firstName")] string FirstName,
	[property: JsonPropertyName("lastName")] string LastName,
	[property: JsonPropertyName("birthDate")] object BirthDate,
	[property: JsonPropertyName("age")] int Age,
	[property: JsonPropertyName("height")] UserHeight _userHeight,
	[property: JsonPropertyName("weight")] int Weight,
	[property: JsonPropertyName("focus")] string Focus,
	[property: JsonPropertyName("email")] string Email
);

public record UserHeight(
	[property: JsonPropertyName("feet")] int Feet,
	[property: JsonPropertyName("inches")] int Inches
);
