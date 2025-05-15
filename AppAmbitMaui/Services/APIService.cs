using AppAmbit.Models.Logs;
using AppAmbit.Services.Auth;
using AppAmbit.Services.ExceptionsCustom;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace AppAmbit.Services;

internal class APIService : IAPIService
{
    private string? _token;
    private bool isRefreshingToken = false;
    private TaskCompletionSource<bool>? _tokenRefreshCompletionSource;

    private readonly List<IEndpoint> endpointsQueue = new();
    private readonly List<IEndpoint> immediateRetries = new();
    public async Task<T?> ExecuteRequest<T>(IEndpoint endpoint) where T : notnull
    {
        try
        {

            HttpResponseMessage responseMessage = await RequestHttp(endpoint);

            Debug.WriteLine($"StatusCode:{(int)responseMessage.StatusCode} {responseMessage.StatusCode}");

            CheckStatusCodeFrom(responseMessage.StatusCode);

            var responseString = await responseMessage.Content.ReadAsStringAsync();
            Debug.WriteLine($"responseString:{responseString}");

            return TryDeserializeJson<T>(responseString);
        }
        catch (UnauthorizedException)
        {
            if (!isRefreshingToken)
            {
                isRefreshingToken = true;
                _tokenRefreshCompletionSource = new TaskCompletionSource<bool>();
                Debug.WriteLine("[APIService] Unauthorized: starting token refresh...");
                _ = RefreshToken();
            }

            Debug.WriteLine("[APIService] Token is already refreshing. Enqueuing endpoint.");

            return await ProceedWithExecutionAfterTokenIsRefreshed<T>(endpoint);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Exception:{e.Message}");
            return default;
        }
    }
    private async Task RefreshToken()
    {
        bool success = await TokenService.TryRefreshTokenAsync();

        if (!success)
        {
            Debug.WriteLine("[APIService] Token refresh FAILED. Clearing pending endpoints.");
            endpointsQueue.Clear();
            immediateRetries.Clear();
            isRefreshingToken = false;
            _tokenRefreshCompletionSource?.SetResult(false);
            return;
        }

        Debug.WriteLine($"[APIService] Token refreshed successfully. Retrying {endpointsQueue.Count} endpoints.");

        foreach (var endpoint in endpointsQueue)
        {
            if (immediateRetries.Contains(endpoint))
            {
                Debug.WriteLine($"[APIService] Skipping already retried endpoint: {endpoint.GetType().Name}");
                continue;
            }

            try
            {
                Debug.WriteLine($"[APIService] Retrying endpoint: {endpoint.GetType().Name}");
                await ExecuteRequest<object>(endpoint);

                endpointsQueue.Remove(endpoint);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[APIService] Failed retrying endpoint {endpoint.GetType().Name}: {e.Message}");
            }
        }

        immediateRetries.Clear();
        isRefreshingToken = false;
        _tokenRefreshCompletionSource?.SetResult(true);
        Debug.WriteLine("[APIService] All pending endpoints processed.");
    }

    private async Task<T?> ProceedWithExecutionAfterTokenIsRefreshed<T>(IEndpoint endpoint)
    {
        Debug.WriteLine($"[APIService] Adding endpoint to queue: {endpoint.GetType().Name}");

        if (!endpointsQueue.Contains(endpoint))
        {
            endpointsQueue.Add(endpoint);
        }

        immediateRetries.Add(endpoint);

        bool refreshSuccess = await _tokenRefreshCompletionSource!.Task;

        if (!refreshSuccess)
        {
            Debug.WriteLine("[APIService] Token refresh failed. Cannot proceed with execution.");
            throw new Exception("Token refresh failed.");
        }

        Debug.WriteLine($"[APIService] Retrying endpoint after token refresh: {endpoint.GetType().Name}");

        endpointsQueue.Remove(endpoint);

        return await ExecuteRequest<T>(endpoint);
    }

    private async Task<HttpResponseMessage> RequestHttp(IEndpoint endpoint)
    {
        var httpClient = new HttpClient()
        {
            Timeout = TimeSpan.FromMinutes(2),
        };
        httpClient.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var responseMessage = await HttpResponseMessage(endpoint, httpClient);
        return responseMessage;
    }

    private void CheckStatusCodeFrom(HttpStatusCode code)
    {
        if (HttpStatusCode.Unauthorized == code)
        {
            throw new UnauthorizedException();
        }
    }


    public string? GetToken()
    {
        return _token;
    }

    public void SetToken(string? token)
    {
        _token = token;
    }

    private T TryDeserializeJson<T>(string response)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(response);
        }
        catch (JsonException)
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
        var data = JsonConvert.SerializeObject(payload, options);
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
                    result = await client.PostAsync(url, payloadJson);
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