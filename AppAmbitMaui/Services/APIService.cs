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
            log.Type = LogType.Crash;
            content = SerializeToMultipartFormDataContent(log);
            await DebugMultipartFormDataContent(content as MultipartFormDataContent);
            log.file = null;
            var data = JsonConvert.SerializeObject(payload);
            Debug.WriteLine($"data:{data}");
        }
        else
        {
            content = SerializeToJSONStringContent(payload);
        }
        return content;
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
        {
            return null;
        }
        
        object logTypeValue = null;
        string logTypeJsonValue = null;

        // First pass to extract type early
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
            
            /*if (property.PropertyType == typeof(Dictionary<string, string>))
            {
                propertyValue = JsonConvert.SerializeObject(propertyValue);
            }*/

            if (propertyName == "file")
            {
                if (logTypeJsonValue != "crash")
                    continue;
                
                var dateFormat = "yyyy-MM-ddTHH_mm_ss_fffZ";
                var fileName = $"log-{DateTime.Now.ToUniversalTime().ToString(dateFormat)}.txt";
                var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                var encodedBytes = Encoding.ASCII.GetBytes(propertyValue as string ?? "");
                //var fileContent = new StreamContent(encodedBytes);
                var fileContent = new ByteArrayContent(encodedBytes);
                //fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                formData.Add(fileContent, "file", Path.GetFileName(filePath));
                continue;
            }
            
            if (propertyValue != null)
            {
                string serializedValue;

                var type = property.PropertyType;
                var isNullable = Nullable.GetUnderlyingType(type) != null;
                var actualType = isNullable ? Nullable.GetUnderlyingType(type) : type;
                
                if (propertyValue is Dictionary<string, string> dict)
                {
                    var arrayRepresentation = dict.Select(kvp => new { key = kvp.Key, value = kvp.Value }).ToArray();
                    serializedValue = JsonConvert.SerializeObject(arrayRepresentation);
                    Debug.WriteLine($"property serializedValue: {propertyName}:{serializedValue}");
                    var count = 0;
                    foreach (var item in arrayRepresentation)
                    {
                        var value = JsonConvert.SerializeObject(item.value);
                        var propertyArrayName = $"{propertyName}[{count++}].{item.key}";
                        formData.Add(new StringContent(value),propertyArrayName);
                    }
                }
                
                if (actualType != null && actualType.IsEnum)
                {
                    // Try to get EnumMember value
                    var enumValue = Enum.Parse(actualType, propertyValue.ToString());
                    var enumMember = actualType.GetMember(enumValue.ToString()).FirstOrDefault();
                    var enumAttr = enumMember?.GetCustomAttribute<EnumMemberAttribute>();
                    serializedValue = enumAttr?.Value ?? enumValue.ToString();
                    Debug.WriteLine($"property serializedValue: {propertyName}:{serializedValue}");
                    formData.Add(new StringContent($"\"{serializedValue}\"", Encoding.UTF8), propertyName);
                }
                else
                {
                    var json = JsonConvert.SerializeObject(propertyValue);
                    
                    Debug.WriteLine($"property json: {propertyName}:{json}");
                    formData.Add(new StringContent(json, Encoding.UTF8, "application/json"), propertyName);
                }
            }
        }
        return formData;
    }
    
    private async Task DebugMultipartFormDataContent(MultipartFormDataContent formData)
    {
        foreach (var content in formData)
        {
            Debug.WriteLine("Headers:");
            foreach (var header in content.Headers)
            {
                if (header.Key == "file")
                    continue;
                Debug.WriteLine($"{header.Key}: {string.Join(", ", header.Value)}");
            }

            var body = await content.ReadAsStringAsync();
            Debug.WriteLine("Body:");
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