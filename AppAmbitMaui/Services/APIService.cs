using AppAmbit.Models.Logs;
using AppAmbit.Models.Responses;
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
    private Task<ApiErrorType>? currentTokenRenewalTask;
    private static double _requestSize = 0;
    public static double RequestSize { get => _requestSize; set => _requestSize = value; }

    public async Task<ApiResult<T>?> ExecuteRequest<T>(IEndpoint endpoint) where T : notnull
    {
        if (!HasInternetConnection())
        {
            Debug.WriteLine("[APIService] Offline - Cannot send request.");
            return ApiResult<T>.Fail(ApiErrorType.NetworkUnavailable, "No internet available");
        }

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
            if (endpoint is RegisterEndpoint)
            {
                Debug.WriteLine("[APIService] Token renew endpoint also failed. Session and Token must be cleared");
                ClearToken();
                return default;
            }

            if (!IsRenewingToken())
            {
                try
                {
                    Debug.WriteLine("[APIService] Token invalid - triggering renewal");
                    currentTokenRenewalTask = GetNewToken();
                    var tokenRenewalResult = await currentTokenRenewalTask;

                    if (!IsRenewSuccess(tokenRenewalResult))
                    {
                        return HandleFailedRenewalResult<T>(tokenRenewalResult);
                    }
                }
                catch (Exception ex)
                {
                    return HandleTokenRenewalException<T>(ex);
                }
                finally
                {
                    currentTokenRenewalTask = null;
                }
            }

            Debug.WriteLine("[APIService] Retrying request after token renewal");
            return await ExecuteRequest<T>(endpoint);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[APIService] Exception during request: {ex}");
            return ApiResult<T>.Fail(ApiErrorType.Unknown, "Unexpected error during request");
        }
    }

    private bool IsRenewingToken()
    {
        return currentTokenRenewalTask != null;
    }

    private bool IsRenewSuccess(ApiErrorType result)
    {
        return result == ApiErrorType.None;
    }

    private ApiResult<T> HandleTokenRenewalException<T>(Exception ex)
    {
        Debug.WriteLine($"[APIService] Error while renewing token: {ex}");
        ClearToken();
        return ApiResult<T>.Fail(ApiErrorType.Unknown, "Unexpected error during token renewal");
    }

    private ApiResult<T>? HandleFailedRenewalResult<T>(ApiErrorType result)
    {
        if (result == ApiErrorType.NetworkUnavailable)
        {
            Debug.WriteLine("[APIService] Cannot retry request: no internet after token renewal");
            return ApiResult<T>.Fail(ApiErrorType.NetworkUnavailable, "No internet after token renewal");
        }

        Debug.WriteLine($"[APIService] Could not renew token. Cleaning up");
        return ApiResult<T>.Fail(result, "Token renewal failed");
    }

    public async Task<ApiErrorType> GetNewToken(string appKey = "")
    {

        try
        {
            var registerEndpoint = await ConsumerService.RegisterConsumer(appKey);
            var tokenResponse = await ExecuteRequest<TokenResponse>(registerEndpoint);

            if (tokenResponse == null)
                return ApiErrorType.Unknown;

            if (tokenResponse.ErrorType == ApiErrorType.NetworkUnavailable)
                return ApiErrorType.NetworkUnavailable;


            if (tokenResponse.ErrorType == ApiErrorType.None)
            {
                _token = tokenResponse.Data?.Token;
                return ApiErrorType.None;
            }

            Debug.WriteLine($"[APIService] Token renew failed: {tokenResponse.ErrorType}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[APIService] Exception during token renew attempt: {ex}");
        }

        return ApiErrorType.Unknown;
    }

    private void ClearToken()
    {
        Debug.WriteLine("[APIService] Session is no longer valid. Clearing token.");
        _token = null;
    }

    private async Task<HttpResponseMessage> RequestHttp(IEndpoint endpoint)
    {
        var handler = new HttpClientHandler();
        var loggingHandler = new LoggingHandler(handler);
        var httpClient = new HttpClient(loggingHandler)
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
        int statusCode = (int)code;

        if (IsSuccessStatusCode(statusCode))
        {
            return;
        }

        if (HttpStatusCode.Unauthorized == code)
        {
            throw new UnauthorizedException();
        }

        throw new HttpRequestException($"HTTP error {statusCode}: {code}");
    }

    private bool IsSuccessStatusCode(int statusCode)
    {
        return statusCode >= 200 && statusCode < 300;
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

    internal static async Task CalculateRequestSize(HttpRequestMessage request)
    {
        int headersSize = 0;
        int bodySize = 0;

        Debug.WriteLine("[APIService] - HEADERS:");
        foreach (var header in request.Headers)
        {
            var headerLine = $"{header.Key}: {string.Join(", ", header.Value)}";
            Debug.WriteLine($"[APIService] - HEADER: {headerLine}");
            headersSize += Encoding.UTF8.GetByteCount(headerLine + "\r\n");
        }

        if (request.Content != null)
        {
            foreach (var header in request.Content.Headers)
            {
                var headerLine = $"{header.Key}: {string.Join(", ", header.Value)}";
                Debug.WriteLine($"[APIService] - CONTENT HEADER: {headerLine}");
                headersSize += Encoding.UTF8.GetByteCount(headerLine + "\r\n");
            }

            var content = await request.Content.ReadAsByteArrayAsync();
            bodySize = content.Length;
        }

        RequestSize = headersSize + bodySize;
        var result = $"{RequestSize:F4}";

        Debug.WriteLine($"[APIService] - TOTAL SIZE: {result} BYTES");
    }

    public double GetRequestSize()
    {
        return _requestSize;
    }

    private bool HasInternetConnection() =>
        Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

}
