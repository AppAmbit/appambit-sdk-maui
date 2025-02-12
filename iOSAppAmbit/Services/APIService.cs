using System.Net.Http.Headers;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using iOSAppAmbit.Services.Base;
using Newtonsoft.Json;
using Shared.Models.Endpoints;
using Shared.Models.Endpoints.Base;
using Microsoft.Extensions.DependencyInjection;

namespace iOSAppAmbit.Services;

public class APIService : IAPIService
{
    public async Task<T> ExecuteRequest<T>(IEndpoint endpoint)
    {
        using var client = new HttpClient();
        
        var responseMessage = await HttpResponseMessage(endpoint, client);
        Console.WriteLine(endpoint.GetType());
        Console.WriteLine(responseMessage.StatusCode);
        var responseString = await responseMessage.Content.ReadAsStringAsync();
        
        return TryDeserializeJson<T>(responseString);
    }
    
    private T TryDeserializeJson<T>(string response)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(response);
        }
        catch (JsonException ex)
        {
            var exceptionMessage = "Could not parse JSON. Something went wrong.";

            throw new JsonException(exceptionMessage);
        }
    }

    private async Task<HttpResponseMessage> HttpResponseMessage(IEndpoint endpoint, HttpClient client)
    {
        client.Timeout = TimeSpan.FromSeconds(20);
        await AddAuthorizationHeaderIfNeeded(client);
        
        var fullUrl = endpoint.BaseUrl + endpoint.Url;
        return await GetHttpResponseMessage(endpoint, client, fullUrl, endpoint.Payload);
    }
    
    
    private async Task AddAuthorizationHeaderIfNeeded(HttpClient client)
    {
        var storageService = Core.Services.GetService<IStorageService>();
        var token = await storageService?.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<HttpContent> SerializeJSONPayload(object payload, IEndpoint endpoint = null)
    {
        if (payload == null)
        {
            return null;
        }
        
        if (endpoint is SendLogsAndSummaryEndpoint)
        {
            var formData = new MultipartFormDataContent();
            foreach (var property in payload.GetType().GetProperties())
            {
                var propertyName = property.Name;
                var propertyValue = property.GetValue(payload);
                if (propertyName == "logFile")
                {
                    var filePath = Path.Combine(NSFileManager.DefaultManager.GetUrls(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User).FirstOrDefault()?.Path, "logs.txt");
                    var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                    formData.Add(fileContent, "logFile", Path.GetFileName(filePath));
                    continue;
                }
                if (propertyValue != null)
                {
                    var json = JsonConvert.SerializeObject(propertyValue);
                    formData.Add(new StringContent(json, Encoding.UTF8, "application/json"), propertyName);
                }
            }
            return formData;
        }

        var data = JsonConvert.SerializeObject(payload);
        var content = new StringContent(data, Encoding.UTF8, "application/json");
        return content;
    }

    private string SerializeStringPayload(object payload)
    {
        if (payload == null)
        {
            return null;
        }

        var serializedPayload = payload.GetType()
            .GetRuntimeProperties()
            .Where(pi => pi.GetValue(payload) != null)
            .Aggregate("", (result, pi) => result
                                           + Uri.EscapeDataString(pi.Name)
                                           + "="
                                           + Uri.EscapeDataString((String)(pi.GetValue(payload)))
                                           + "&");
        return serializedPayload.Substring(0, serializedPayload.Length - 1);
    }

    private string SerializedGetURL(string url, object payload)
    {
        var serializedParameters = SerializeStringPayload(payload);
        if (serializedParameters == null)
        {
            return url;
        }

        return url + "?" + serializedParameters;
    }

    private async Task<HttpResponseMessage> GetHttpResponseMessage(IEndpoint endpoint, HttpClient client, string url, object payload)
    {
        HttpResponseMessage result;
        try
        {
            switch (endpoint.Method)
            {
                case HttpMethodEnum.Get:
                    result = await client.GetAsync(SerializedGetURL(url, payload));
                    break;
                case HttpMethodEnum.Post:
                    result = await client.PostAsync(url, await SerializeJSONPayload(payload, endpoint));
                    break;
                case HttpMethodEnum.Patch:
                    var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                    {
                        Content = await SerializeJSONPayload(payload)
                    };
                    result = await client.SendAsync(requestMessage);
                    break;
                case HttpMethodEnum.Put:
                    result = await client.PutAsync(url, await SerializeJSONPayload(payload));
                    break;
                case HttpMethodEnum.Delete:
                    result = await client.DeleteAsync(url);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (TaskCanceledException)
        {
            throw new Exception();
        }
        return result;
    }
}