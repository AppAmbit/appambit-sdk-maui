using System.Diagnostics;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;

namespace AppAmbit.Services;

internal class APIService : IAPIService
{
    private string? _token;
    
    public async Task<T> ExecuteRequest<T>(IEndpoint endpoint)
    {
        var httpClient = new HttpClient(){
            Timeout = TimeSpan.FromMinutes(2),
        };
        httpClient.DefaultRequestHeaders
            .Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        
        var responseMessage = await HttpResponseMessage(endpoint, httpClient);
        Debug.WriteLine($"StatusCode:{(int)responseMessage.StatusCode} {responseMessage.StatusCode}");
        var responseString = await responseMessage.Content.ReadAsStringAsync();
        Debug.WriteLine($"responseString:{responseString}");
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
            content = SerializeToMultipartFormDataContent(log);
            DebugMultipartFormDataContent(content as MultipartFormDataContent);
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
        log.file = "_FILE_";
        var data = JsonConvert.SerializeObject(log);
        Debug.WriteLine($"data:{data}");
    }

    private static HttpContent SerializeToJSONStringContent(object payload)
    {
        
        var data = JsonConvert.SerializeObject(payload);
        Debug.WriteLine($"data:{data}");
        var content = new StringContent(data, Encoding.UTF8, "application/json");
        return content;
    }
    
    private HttpContent SerializeToMultipartFormDataContent(object payload)
    {
        var formData = new MultipartFormDataContent();

        if (payload == null)
            return null;

        object logTypeValue = null;
        string logTypeJsonValue = null;
        
        foreach (var prop in payload.GetType().GetProperties())
        {
            var jsonPropAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
            var propName = jsonPropAttr?.PropertyName ?? prop.Name;

            if (propName == "type")
            {
                logTypeValue = prop.GetValue(payload);
                var actualType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                if (actualType.IsEnum)
                {
                    var enumVal = Enum.Parse(actualType, logTypeValue.ToString());
                    var enumMember = actualType.GetMember(enumVal.ToString()).FirstOrDefault();
                    var enumAttr = enumMember?.GetCustomAttribute<EnumMemberAttribute>();
                    logTypeJsonValue = enumAttr?.Value ?? enumVal.ToString();
                }
            }
        }

        foreach (var property in payload.GetType().GetProperties())
        {
            var jsonIgnoreAttribute = property.GetCustomAttribute<JsonIgnoreAttribute>();
            if (jsonIgnoreAttribute != null)
                continue;
            
            var jsonPropertyAttribute = property.GetCustomAttribute<JsonPropertyAttribute>();
            var propertyName = jsonPropertyAttribute?.PropertyName ?? property.Name;
            var propertyValue = property.GetValue(payload);
            
            if (propertyName == "file")
            {
                if (logTypeJsonValue != "crash")
                    continue;

                var dateFormat = "yyyy-MM-ddTHH_mm_ss_fffZ";
                var fileName = $"log-{DateTime.Now.ToUniversalTime().ToString(dateFormat)}.txt";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                var encodedBytes = Encoding.ASCII.GetBytes(propertyValue as string ?? "");
                var fileContent = new ByteArrayContent(encodedBytes);
                formData.Add(fileContent, "file", Path.GetFileName(filePath));
                continue;
            }
            
            if (propertyName == "context" && propertyValue is Dictionary<string,string> contextDict)
            {
                int count = 0;
                foreach (var kvp in contextDict)
                {
                    var key = kvp.Key;
                    var value = kvp.Value?.ToString() ?? "";
                    var contextKey = $"context[{count++}].{key}";
                    formData.Add(new StringContent(value), contextKey);
                }
                continue;
            }
            
            var type = property.PropertyType;
            var isNullable = Nullable.GetUnderlyingType(type) != null;
            var actualType = isNullable ? Nullable.GetUnderlyingType(type) : type;

            if (actualType != null && actualType.IsEnum && propertyValue != null)
            {
                var enumVal = Enum.Parse(actualType, propertyValue.ToString());
                var enumMember = actualType.GetMember(enumVal.ToString()).FirstOrDefault();
                var enumAttr = enumMember?.GetCustomAttribute<EnumMemberAttribute>();
                var enumValueStr = enumAttr?.Value ?? enumVal.ToString();
                formData.Add(new StringContent(enumValueStr), propertyName);
                continue;
            }
            
            if (propertyValue != null)
            {
                var stringValue = propertyValue.ToString();
                formData.Add(new StringContent(stringValue), propertyName);
            }
        }

        return formData;
    }
    
    [Conditional("DEBUG")]
    private async void DebugMultipartFormDataContent(MultipartFormDataContent formData)
    {
        foreach (var content in formData)
        {
            Debug.WriteLine("Headers:");
            foreach (var header in content.Headers)
            {
                if (header.Key == "file")
                {
                    Debug.WriteLine($"{header.Key}: FILE");
                    continue;
                }
                
                Debug.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            var body = await content.ReadAsStringAsync();
            Debug.WriteLine("Body:");
            Debug.WriteLine("----------------------------");
            Debug.WriteLine(body);
            Debug.WriteLine("----------------------------");
        }
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
    
    public string? GetToken()
    {
        return _token;
    }
    
    public void SetToken( string? token)
    {
        _token = token;
    }
}