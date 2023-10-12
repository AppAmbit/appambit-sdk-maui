using System.ComponentModel;
using Kava.HttpClient.Authentication;
using OneOf;

namespace Kava.HttpClient;

public class WebApiClient :
	IEndpointSelector,
	IParameterSelector,
	IExecutionSelector
{
	public WebApiClientOptions Options { get; private set; }

	// TODO: Fix issue with having a factory method but also needing DI
	public WebApiClient(IHttpClientFactory httpClientFactory)
	{
		Options = new WebApiClientOptions
		{
			Client = httpClientFactory.CreateClient(),
			Request = new(),
			Url = null,
		};
	}
	 private WebApiClient()
	 {}

	public static IEndpointSelector CreateClient()
	{
		return new WebApiClient();
	}
	
	// 1. Endpoint Selection Stage
	public IParameterSelector Get(OneOf<string, Uri> url)
	{
		Options.Url = url.Match(
			str => str,
			uri => uri.AbsoluteUri
		);

		Options.Request = new HttpRequestMessage(HttpMethod.Get, Options.Url);

		return this;
	}

	public IParameterSelector Post(OneOf<string, Uri> url)
	{
		Options.Url = url.Match(
			str => str,
			uri => uri.AbsoluteUri
		);

		Options.Request = new HttpRequestMessage(HttpMethod.Get, Options.Url);
		
		return this;
	}
	
	public IParameterSelector Patch(OneOf<string, Uri> url)
	{
		Options.Url = url.Match(
			str => str,
			uri => uri.AbsoluteUri
		);

		Options.Request = new HttpRequestMessage(HttpMethod.Patch, Options.Url);
		
		return this;
	}
	
	public IParameterSelector Put(OneOf<string, Uri> url)
	{
		Options.Url = url.Match(
			str => str,
			uri => uri.AbsoluteUri
		);

		Options.Request = new HttpRequestMessage(HttpMethod.Put, Options.Url);
		
		return this;
	}

	public IParameterSelector Delete(OneOf<string, Uri> url)
	{
		Options.Url = url.Match(
			str => str,
			uri => uri.AbsoluteUri
		);

		Options.Request = new HttpRequestMessage(HttpMethod.Delete, Options.Url);
		
		return this;
	}

	// 2. Parameter Selection Stage
	public IExecutionSelector WithNoParams()
	{
		Options.Request.Content?.Headers.Clear();
		return this;
	}
	
	public IExecutionSelector WithBodyParam(string name, string value)
	{
		Options.Request.Content?.Headers.Clear();
		Options.Request.Content?.Headers.Add(name, value);
		return this;
	}
	
	public IExecutionSelector WithContentHeaderField(string name, string value)
	{
		_ = WithBodyParam(name, value);
		return this;
	}
	
	public IExecutionSelector WithBodyParams(OneOf<object, ICollection<KeyValuePair<string, string>>> parameters)
	{
		parameters.Switch(
			obj =>
			{
				foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(obj))
				{
					var value = prop.GetValue(obj);
					if (value is string strValue)
					{
						Options.Request.Content?.Headers.Add(prop.Name, strValue);
					}
				}
			},
			dict =>
			{
				foreach (var entry in dict)
				{
					Options.Request.Content?.Headers.Add(entry.Key, entry.Value);
				}
			});

		return this;
	}

	public IExecutionSelector WithContentHeaderFields(
		OneOf<object, ICollection<KeyValuePair<string, string>>> parameters)
	{
		_ = WithBodyParams(parameters);
		return this;
	}

	public IExecutionSelector WithNoHeaderParams()
	{
		
		return this;
	}
	

	public IExecutionSelector WithHeaderParam(string name, OneOf<string> value)
	{
		var resolvedValue = value.Match(
			str => str
		);
		
		Options.Request.Headers.Add(name, resolvedValue);
		return this;
	}

	public IExecutionSelector WithHeaderParams(OneOf<object, ICollection<KeyValuePair<string, string>>> headers)
	{
		headers.Switch(
			obj =>
			{
				foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(obj))
				{
					var value = prop.GetValue(obj);
					if (value is string strValue)
					{
						Options.Request.Headers.Add(prop.Name, strValue);
					}
				}
			},
			collection =>
			{
				foreach (var entry in collection)
				{
					Options.Request.Headers.Add(entry.Key, entry.Value);
				}
			}
		);
		
		return this;
	}

	public IExecutionSelector WithQueryParam(string name, OneOf<string> value)
	{
		var resolvedValue = value.Match(
			str => str
		);
		
		// TODO
		
		return this;
	}
	
	public IExecutionSelector WithQueryParams(OneOf<object, ICollection<KeyValuePair<string, string>>> queryParams)
	{
		// TODO
		return this;
	}

	// 3. Execution Stage
	public OneOf<HttpResponseMessage, Exception> Execute()
	{
		// TODO
		return new NotImplementedException();
	}
	
	public OneOf<HttpResponseMessage, Exception> Execute<T>()
		where T : class
	{
		// TODO
		return new NotImplementedException();
	}
}

public record WebApiClientOptions
{
	public required System.Net.Http.HttpClient Client { get; set; }
	public required HttpRequestMessage Request { get; set; }
	public required string? Url { get; set; }
}

public interface IWebApiClientOptions
{
	public WebApiClientOptions Options { get; }
}