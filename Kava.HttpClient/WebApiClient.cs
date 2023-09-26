using System.ComponentModel;
using OneOf;
using RestSharp;
using RestSharp.Authenticators;

namespace Kava.HttpClient;

/*
 * var dogRequest = webApiClient
 *						.GetFrom("https://www.doggos.com/api/dogs?limit=10,page=1")
 *						.Execute();
 *
 * var twitterAuthEndpoint = new TwitterAuthEndPoint()
 *		.WithParams(new { userId = 2})
 *		.UseRefreshStrategy()
 *		.Execute();
 *
 *
 *
 * var dogRequest2 = webApiClient
 *						.GetFrom("https://www.doggos.com/api/dogs")
 *						.WithParams(new { limit = 10, page = 1 })
 *						.MapTo<List<Dog>>()
 *						.Execute();
 *
 * var dogRequest3 = webApiClient
 *						.GetFrom("https://www.doggos.com/api/dogs")
 *						.WithParams(myParamsObjThatImplsParameterable)
 *						.WithAutentication("https://www.doggos.com/api", apiKey, apiKeySecret)
 *						.TryRefresh<RefreshStrategy>()
 *						.AttemptRetries(5)
 *						.UseKavaRefresh()
 *						.WithHeaders()
 *						.Execute();
 *
 * var request4 = webApiClient
 *						.GetFrom("https://www.doggos.com/api/dogs")
 *						.WithParams(new { limit = 10, page = 1 });
 *
 * Pros of endpoints: Queueing multiple calls from the same endpoint
 *
 * ability to behind-the-scenes/automatically refresh the token for endpoints that require authN
 *
 * use case: user goes to recipe page, hasn't used app in a while, they refresh
 * but their request to the recipe page drops and they get shown a blank page,
 * harming their experience
 *
 * good to build: Try to create Authenticators with:
 *		- Auth0
 *		- HttpBasic
 *		- Our own custom provider sink
 *
 * TODO: Move the underlying RestSharp implementation to its own concrete subtype
 * TODO: Put in its own solution (see projects like Serilog or Rebus)
 * Examples:
 *		Kava.HttpClient (This impl can stay here)
 *		Kava.HttpClient.RestSharp
 *		Kava.HttpClient.AnotherImpl
 * https://www.nuget.org/packages?q=microsoft.extensions.configuration
 * https://www.nuget.org/packages?q=serilog
 * https://www.nuget.org/packages?q=rebus
 */

public class WebApiClient : IDisposable
{
	private RestClient Client { get; set; } = new();

	private RestClientOptions ClientOptions { get; set; } = new()
	{
		FailOnDeserializationError = true,
	};
	
	private RestRequest? Request { get; set; }
	public string? Url { get; private set; }
	public Method Method { get; private set; }
	public Dictionary<string, string>? Headers { get; private set; }
	
	public AuthenticatorBase? Authenticator { get; private set; }
	public OneOf<string, Uri>? AuthUrl { get; private set; }
	
	public Dictionary<string, string>? Parameters { get; private set; }

	public WebApiClient Get(OneOf<string, Uri> url)
	{
		Url = url.Match(
			str => str,
			uri => uri.AbsoluteUri
		);

		Method = Method.Get;
		Request = new RestRequest(Url, Method);

		return this;
	}

	public WebApiClient Post(OneOf<string, Uri> url)
	{
		Url = url.Match(
			str => str,
			uri => uri.AbsoluteUri
		);

		Method = Method.Post;
		Request = new RestRequest(Url, Method);
		
		return this;
	}
	
	public WebApiClient WithParams(OneOf<object, Dictionary<string, string>> parameters)
	{
		var resolvedParams = parameters.Match(
			obj =>
			{
				var dict = new Dictionary<string, string>();
				foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(obj))
				{
					var value = prop.GetValue(obj);
					if (value is string strValue)
					{
						dict.Add(prop.Name, strValue);
					}
				}
				return dict;
			},
			dict => dict
		);
		
		Parameters = resolvedParams;
		return this;
	}

	public WebApiClient WithHeader(string name, OneOf<string> value)
	{
		var resolvedValue = value.Match(
			str => str
		);
		
		Request?.AddOrUpdateHeader(name, resolvedValue);
		
		Headers ??= new();
		Headers.Add(name, resolvedValue);
		
		return this;
	}

	public WebApiClient WithJsonBody(OneOf<object, string> json)
	{
		json.Switch(
			obj => Request?.AddJsonBody(obj),
			str => Request?.AddJsonBody(str)
		);
		
		return this;
	}

	public WebApiClient UseAuthenticator(string baseUrl, string email, string password)
	{
		Authenticator = new Tb12Authenticator(baseUrl, email, password);
		return this;
	}
	
	public OneOf<RestResponse, Exception> Execute()
	{
		if (Request is null)
		{
			return new NullReferenceException(
				"""
				The request for this WebClient is null.
				See if you've called .Get() or .Post() with a valid URL.
				""");
		}

		return Client.Execute(Request, Method);
	}
	
	public OneOf<RestResponse<T>, Exception> Execute<T>()
		where T : class
	{
		if (Request is null)
		{
			return new NullReferenceException(
				"""
				The request for this WebClient is null.
				See if you've called .Get() or .Post() with a valid URL.
				""");
		}

		if (Authenticator is not null)
		{
			ClientOptions.Authenticator = Authenticator;
			Client = new RestClient(ClientOptions);
		}

		return Client.Execute<T>(Request, Method);
	}

	public void Dispose()
	{
		Client.Dispose();
		GC.SuppressFinalize(this);
	}
}