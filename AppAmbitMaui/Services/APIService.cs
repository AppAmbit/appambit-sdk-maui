using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using AppAmbit.Models.App;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit.Services;

internal class APIService : IAPIService
{
    private string? _token;
    private bool _tokenWasRefreshed = false;
    private SemaphoreSlim _semaphoreRequests = new SemaphoreSlim(1, 1);
    public async Task<T?> ExecuteRequest<T>(IEndpoint endpoint) where T: notnull
    {
        await _semaphoreRequests.WaitAsync();
        try
        {
            using var httpClient = CreateHttpClient();
            var responseMessage = await HttpResponseMessage(endpoint, httpClient);

            if (TokenIsExpired((int)responseMessage.StatusCode) && !_tokenWasRefreshed)
            {
                Debug.WriteLine($"StatusCode:{(int)responseMessage.StatusCode} {responseMessage.StatusCode}");
                var refreshed = await RefreshToken(endpoint.Payload);
                if (refreshed)
                {
                    using var retryClient = CreateHttpClient();
                    responseMessage = await HttpResponseMessage(endpoint, retryClient);
                }
            }

            _tokenWasRefreshed = false;
            Debug.WriteLine($"StatusCode:{(int)responseMessage.StatusCode} {responseMessage.StatusCode}");
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            Debug.WriteLine($"responseString:{responseString}");
            return TryDeserializeJson<T>(responseString);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception:{e.Message}");
            return default;
        }
        finally
        {
            _semaphoreRequests.Release();
        }
    }

    private HttpClient CreateHttpClient()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };

        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var token = GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    private async Task<bool> RefreshToken(object payload)
    {
        try
        {
            _tokenWasRefreshed = true;
            await Core.InitializeConsumer();
            Debug.Write("Refreshed token: " + GetToken());
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error refreshing token: {e.Message}");
            return false;
        }
    }

    private bool TokenIsExpired(int status)
    {
        return ((int)HttpStatusCode.Unauthorized) == status;
    }
    
    public string? GetToken()
    {
        return _token;
    }
    
    public void SetToken( string? token)
    {
        _token = token;
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
        using var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(20)
        };
        await AddAuthorizationHeaderIfNeeded(httpClient);

        var fullUrl = endpoint.BaseUrl + endpoint.Url;
        return await GetHttpResponseMessage(endpoint, client, fullUrl, endpoint.Payload);
    }
    
    private async Task AddAuthorizationHeaderIfNeeded(HttpClient client)
    {
        var token = GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private async Task<HttpContent> SerializePayload(object payload, IEndpoint endpoint = null)
    {
        if (payload == null)
        {
            return null;
        }
        
        HttpContent content;
        if (payload is Log log)
        {
            PrintLogWithoutFile(log);
            var multipartFormDataContent = SerializeToMultipartFormDataContent(log);
            content = multipartFormDataContent;
            
        }
        else if (payload is LogBatch logBatch)
        {
            var multipartFormDataContent = SerializeToMultipartFormDataContent(logBatch);
            content = multipartFormDataContent;
        }
        else
        {
            content = SerializeToJSONStringContent(payload);
        }
        return content;
    }

    [Conditional("DEBUG")]
    private static void PrintLogWithoutFile(Log log)
    {
        log.File = "_FILE_";
        var data = JsonConvert.SerializeObject(log);
        Debug.WriteLine($"data:{data}");
    }

    private static HttpContent SerializeToJSONStringContent(object payload)
    {
        var options = new JsonSerializerSettings() 
        {
            NullValueHandling = NullValueHandling.Ignore,
        };
        var data = JsonConvert.SerializeObject(payload,options);
        Debug.WriteLine($"data:{data}");
        var content = new StringContent(data, Encoding.UTF8, "application/json");
        return content;
    }
    
    private MultipartFormDataContent SerializeToMultipartFormDataContent(object payload)
    {
        Debug.WriteLine("SerializeToMultipartFormDataContent");
        var formData = new MultipartFormDataContent();
        formData.AddObjectToMultipartFormDataContent(payload);
        return formData;
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
                    var payloadJson = await SerializePayload(payload, endpoint);
                    result = await client.PostAsync(url,payloadJson );
                    break;
                case HttpMethodEnum.Patch:
                    var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                    {
                        Content = await SerializePayload(payload)
                    };
                    result = await client.SendAsync(requestMessage);
                    break;
                case HttpMethodEnum.Put:
                    result = await client.PutAsync(url, await SerializePayload(payload));
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