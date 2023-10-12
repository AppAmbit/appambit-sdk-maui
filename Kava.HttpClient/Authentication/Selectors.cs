using OneOf;

namespace Kava.HttpClient.Authentication;

public interface IEndpointSelector : IWebApiClientOptions
{
	public IParameterSelector Get(OneOf<string, Uri> url);
	public IParameterSelector Post(OneOf<string, Uri> url);
	public IParameterSelector Patch(OneOf<string, Uri> url);
	public IParameterSelector Put(OneOf<string, Uri> url);
	public IParameterSelector Delete(OneOf<string, Uri> url);
}

public interface IParameterSelector : IWebApiClientOptions
{
	public IExecutionSelector WithNoParams();
	public IExecutionSelector WithBodyParam(string name, string value);
	public IExecutionSelector WithBodyParams(OneOf<object, ICollection<KeyValuePair<string, string>>> parameters);
	public IExecutionSelector WithHeaderParam(string name, OneOf<string> value);
	public IExecutionSelector WithHeaderParams(OneOf<object, ICollection<KeyValuePair<string, string>>> headers);
	public IExecutionSelector WithQueryParam(string name, OneOf<string> value);
	public IExecutionSelector WithQueryParams(
		OneOf<object, ICollection<KeyValuePair<string, string>>> queryParams);
}

public interface IExecutionSelector : IWebApiClientOptions
{
	public OneOf<HttpResponseMessage, Exception> Execute();
	public OneOf<HttpResponseMessage, Exception> Execute<T>() where T : class;
}