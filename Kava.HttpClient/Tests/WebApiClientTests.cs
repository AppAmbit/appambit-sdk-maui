using System.Diagnostics;
using System.Net;
using Kava.HttpClient.Authentication;
using Xunit;

namespace Kava.HttpClient.Tests;

public class WebApiClientTests
{
	private readonly IEndpointSelector _sut = WebApiClient.CreateClient();
	
	// TODO: New tests to reflect the alternative approach of using basic
	// HttpClient implementation vs using RestSharp
}