using System.Collections;
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
    private const string _dateTimeFormatISO8601ForFile = "yyyy-MM-ddTHH_mm_ss_fffZ";
    private const string _dateFormatStringApi = "yyyy-MM-dd HH:mm:ss";
    
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
        else if (IsCollection ( payload ))
        {
            var list = ToObjectList(payload);
            content = SerializeArrayToMultipartFormDataContent(list);
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
        var options = new JsonSerializerSettings() 
        {
            NullValueHandling = NullValueHandling.Ignore,
        };
        var data = JsonConvert.SerializeObject(payload,options);
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
                
                var fileName = $"log-{DateTime.Now.ToUniversalTime().ToString(_dateTimeFormatISO8601ForFile)}.txt";
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
                var options = new JsonSerializerSettings() 
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatString = _dateFormatStringApi
                };
                var data = JsonConvert.SerializeObject(propertyValue,options);
                formData.Add(new StringContent(data), propertyName);
            }
        }

        return formData;
    }
    
    private HttpContent SerializeArrayToMultipartFormDataContent(List<object> items)
    {
        Debug.WriteLine("SerializeArrayToMultipartFormDataContent");
        var formData = new MultipartFormDataContent();

        for (int index = 0; index < items.Count; index++)
        {
            var item = items[index];
            string logTypeJsonValue = null;

            foreach (var prop in item.GetType().GetProperties())
            {
                var jsonPropAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                var propName = jsonPropAttr?.PropertyName ?? prop.Name;

                if (propName == "type")
                {
                    var logTypeValue = prop.GetValue(item);
                    var actualType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                    if (actualType.IsEnum)
                    {
                        var enumVal = Enum.Parse(actualType, logTypeValue.ToString());
                        var enumMember = actualType.GetMember(enumVal.ToString()).FirstOrDefault();
                        var enumAttr = enumMember?.GetCustomAttribute<EnumMemberAttribute>();
                        logTypeJsonValue = enumAttr?.Value ?? enumVal.ToString();
                    }
                    else
                    {
                        logTypeJsonValue = logTypeValue?.ToString();
                    }
                    break;
                }
            }

            foreach (var prop in item.GetType().GetProperties())
            {
                var jsonIgnoreAttr = prop.GetCustomAttribute<JsonIgnoreAttribute>();
                if (jsonIgnoreAttr != null)
                    continue;

                var jsonPropAttr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                var propName = jsonPropAttr?.PropertyName ?? prop.Name;
                var propValue = prop.GetValue(item);

                string multipartKey = $"{index}[{propName}]";

                if (propName == "file")
                {
                    if (logTypeJsonValue != "crash")
                        continue;

                    if (propValue is string path && File.Exists(path))
                    {
                        var streamContent = new StreamContent(File.OpenRead(path));
                        formData.Add(streamContent, multipartKey, Path.GetFileName(path));
                    }
                    continue;
                }

                if (propName == "context" && propValue is Dictionary<string, string> contextDict)
                {
                    int contextIndex = 0;
                    foreach (var kvp in contextDict)
                    {
                        var key = kvp.Key;
                        var value = kvp.Value ?? string.Empty;
                        var contextKey = $"{index}[context][{contextIndex}].{key}";
                        formData.Add(new StringContent(value), contextKey);
                        contextIndex++;
                    }
                    continue;
                }

                var type = prop.PropertyType;
                var isNullable = Nullable.GetUnderlyingType(type) != null;
                var actualType = isNullable ? Nullable.GetUnderlyingType(type) : type;

                if (actualType != null && actualType.IsEnum && propValue != null)
                {
                    var enumVal = Enum.Parse(actualType, propValue.ToString());
                    var enumMember = actualType.GetMember(enumVal.ToString()).FirstOrDefault();
                    var enumAttr = enumMember?.GetCustomAttribute<EnumMemberAttribute>();
                    var enumStr = enumAttr?.Value ?? enumVal.ToString();
                    formData.Add(new StringContent(enumStr), multipartKey);
                    continue;
                }

                if (propValue != null)
                {
                    //if (propName == "created_at")
                    //    continue;
                    var options = new JsonSerializerSettings() 
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DateFormatString = _dateFormatStringApi
                    };
                    var data = "";
                    data = JsonConvert.SerializeObject(propValue,options);
                    if (propValue is DateTime dateTime )
                    {
                        data = data.Trim('\"');
                    }
                    if (propValue is Guid )
                    {
                        data = data.Trim('\"');
                    }
                    if (propValue is string s)
                    {
                        data = s;
                    }
                    formData.Add(new StringContent(data), multipartKey);
                }
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
    

    private bool IsCollection(object obj)
    {
        if (obj == null) return false;

        var type = obj.GetType();
        return typeof(System.Collections.IEnumerable).IsAssignableFrom(type) && type != typeof(string);
    }
    List<object> ToObjectList(object collection)
    {
        if (collection is not IEnumerable enumerable || collection is string)
            return new List<object>(){collection};

        var list = new List<object>();
        foreach (var item in enumerable)
        {
            list.Add(item);
        }
        return list;
    }
}