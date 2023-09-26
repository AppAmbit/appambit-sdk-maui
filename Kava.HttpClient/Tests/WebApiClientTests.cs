using System.Diagnostics;
using System.Text.Json.Serialization;
using RestSharp;
using RestSharp.Authenticators;
using Xunit;

namespace Kava.HttpClient.Tests;

public class WebApiClientTests
{
	private readonly WebApiClient _sut = new();

	[Fact]
	public void Get_ShouldSetUrlAndMethod()
	{
		var actual = _sut
			.Get("https://staging-api-tb12.kavaup.io/api/focuses");
		
		Assert.Equal("https://staging-api-tb12.kavaup.io/api/focuses", _sut.Url);
		Assert.Equal(Method.Get, _sut.Method);
	}
	
	[Fact]
	public void Get_ShouldReturnSuccessfulJsonResponse()
	{
		var actual = _sut
			.Get("https://staging-api-tb12.kavaup.io/api/focuses")
			.Execute();
		
		actual.Switch(
			response =>
			{
				Assert.True(response.IsSuccessful);
				Assert.Equal("application/json", response.ContentType);
			},
			exception => Assert.Fail("""
			                         Failure due to REST response.
			                         This doesn't necessarily mean that something is
			                         wrong with the impl since returning an error due
			                         to the target API is expected behavior.
			                         """)
		);
	}

	[Fact]
	public void Post_ShouldReturnUserAndTokens()
	{
		var actual = _sut
			.Post("https://staging-api-tb12.kavaup.io/api/users/login")
			.WithJsonBody(new { email = "admin@admin.com", password = "password1" })
			.Execute<Tb12PostLoginResponse>();
		
		actual.Switch(
			response =>
			{
				Assert.True(response.IsSuccessful);
				Assert.True(response.Data is not null);

				var (user, token, refreshToken) = response.Data;
				
				Assert.True(user is not null 
				            && token is not null 
				            && refreshToken is not null);
			},
			exception => Assert.Fail("""
			                        Failure due to REST response.
			                        This doesn't necessarily mean that something is
			                        wrong with the impl since returning an error due
			                        to the target API is expected behavior.
			                        """)
		);
	}

	[Fact]
	public void WithAuthenticator_ShouldPreserveAuthInfo()
	{
		// You could do this, but this isn't as useful since tokens won't persist
		// without capturing them manually from the Response
		// var actual = _sut
		// 	.Post("https://staging-api-tb12.kavaup.io/api/users/login")
		// 	.WithJsonBody(new { email = "admin@admin.com", password = "password1" })
		// 	.Execute();

		var actual = _sut
			.UseAuthenticator(
				"https://staging-api-tb12.kavaup.io",
				"admin@admin.com",
				"password1")
			.Get("https://staging-api-tb12.kavaup.io/api/users/me")
			.Execute<Tb12GetAuthUser>();

		var authenticatedRequests = _sut
			.UseAuthenticator("", "", "");

		authenticatedRequests
			.Get("");

		authenticatedRequests.Get("api2");
		
		actual.Switch(
			response =>
			{
				Assert.True(response.IsSuccessful);
				
				Debug.Assert(response.Data != null, "response.Data != null");
				Assert.Equal(5, response.Data.Height.Feet);
				Assert.Equal(10, response.Data.Height.Inches);
				Assert.Null(response.Data.BirthDate);
			},
			exception => Assert.Fail("Could be something else is wrong")
		);
	}
}