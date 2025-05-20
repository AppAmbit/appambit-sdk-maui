using AppAmbit.Models.Logs;
using AppAmbit.Models.Responses;
using AppAmbit.Services.Auth;
using AppAmbit.Services.ExceptionsCustom;
using AppAmbit.Services.Interfaces;
using AppAmbit.Enums;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using AppAmbit.Services.Endpoints;

namespace AppAmbit.Services;

internal class APIService : IAPIService
{
    private string? _token;
    private bool isTokenBeingRenewed = false;
    private Task? currentTokenRenewalTask;
    private readonly List<IEndpoint> activeRequests = [];
    public async Task<ApiResult<T>?> ExecuteRequest<T>(IEndpoint endpoint) where T : notnull
    {
        if (!HasInternetConnection())
        {
            Debug.WriteLine("[APIService] Offline - Cannot send request.");
            return ApiResult<T>.Fail(ApiErrorType.NetworkUnavailable, "No internet available");
        }

        activeRequests.Add(endpoint);

        try
        {
            var httpResponse = await RequestHttp(endpoint);
            CheckStatusCodeFrom(httpResponse.StatusCode);

            var json = await httpResponse.Content.ReadAsStringAsync();
            var parsed = TryDeserializeJson<T>(json);
            return ApiResult<T>.Success(parsed);
        }
        catch (UnauthorizedException)
        {
            if (!isTokenBeingRenewed)
            {
                Debug.WriteLine("[APIService] Token invalid - triggering renewal.");
                isTokenBeingRenewed = true;
                currentTokenRenewalTask = RenewTokenInBackground();

                Debug.WriteLine($"[APIService] Preparing for retry of: {endpoint.GetType().Name}");
            }
            else if (endpoint is RegisterEndpoint)
            {
                Debug.WriteLine("[APIService] Token refresh endpoint also failed. Session and Token must be cleared.");
                activeRequests.RemoveAll(e => e is RegisterEndpoint);
                _token = null;
                activeRequests.Remove(endpoint);
                return default;
            }
            else
            {
                Debug.WriteLine("[APIService] Awaiting ongoing token renewal...");
                await currentTokenRenewalTask!;
            }

            Debug.WriteLine("[APIService] Retrying request after token renewal.");
            activeRequests.Remove(endpoint);
            return await ExecuteRequest<T>(endpoint);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[APIService] Unhandled error: {ex.Message}");
            return ApiResult<T>.Fail(ApiErrorType.Unknown, ex.Message);
        }
        finally
        {
            activeRequests.Remove(endpoint);
        }
    }


    private async Task RenewTokenInBackground()
    {
        bool renewed = await TokenService.TryRefreshTokenAsync();

        if (!renewed)
        {
            Debug.WriteLine("[APIService] Could not refresh token. Cleaning up.");
            activeRequests.RemoveAll(e => e is RegisterEndpoint);
            ForceSessionReset();
        }
        else
        {
            Debug.WriteLine("[APIService] Token successfully refreshed.");
        }

        isTokenBeingRenewed = false;
    }

    private void ForceSessionReset()
    {
        Debug.WriteLine("[APIService] Session is no longer valid. Clearing token.");
        _token = null;
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

    private bool HasInternetConnection() =>
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;


}

