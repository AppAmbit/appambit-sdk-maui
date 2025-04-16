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
        else if (payload is LogBatch logBatch)
        {
            content = SerializeToMultipartFormDataContent(logBatch);
            DebugMultipartFormDataContent(content as MultipartFormDataContent);
        }
        /*else if (IsCollection ( payload ))
        {
            var list = ToObjectList(payload);
            content = SerializeArrayToMultipartFormDataContent(list);
            DebugMultipartFormDataContent(content as MultipartFormDataContent);
        }*/
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
        Debug.WriteLine("SerializeToMultipartFormDataContent");
        var formData = new MultipartFormDataContent();
        AddObjectToMultipartFormDataContent(payload, formData);
        return formData;
    }
    private void AddObjectToMultipartFormDataContent(object obj, MultipartFormDataContent formData,  string prefix = "", bool useSquareBrakets = false)
    {
        Debug.WriteLine("AddObjectToMultipartFormDataContent");
        
        if (obj is null )
            return;
        
        if (obj is IDictionary dict)
        {
            AddDictionaryToMultipartFormDataContent( dict, formData, prefix);
            return;
        }

        if (IsList ( obj ))
        {
            var list = ToObjectList(obj);
            AddListToMultipartFormDataContent( list, formData, prefix);
            return;
        }

        if ( obj is DateTime dateTime)
        {
            formData.Add(new StringContent(dateTime.ToString(_dateFormatStringApi)), prefix);
            return;
        }

        if (IsSimpleType(obj))
        {
            formData.Add(new StringContent(obj.ToString() ?? "" ), prefix);
            return;
        }
        
        var objectProperties = obj.GetType().GetProperties();
        foreach (var property in objectProperties)
        {
            var propName = property.Name;
            var propValue = property.GetValue(obj);
            
            if (propValue == null)
                continue;
            
            var jsonIgnoreAttr = property.GetCustomAttribute<JsonIgnoreAttribute>();
            if (jsonIgnoreAttr != null)
                continue;

            var jsonPropAttr = property.GetCustomAttribute<JsonPropertyAttribute>();
            propName = jsonPropAttr?.PropertyName ?? property.Name;
            

            string multipartKey = useSquareBrakets ? $"{prefix}[{propName}]":$"{prefix}{propName}";
            var type = property.PropertyType;
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

            var multipartFormDataFileAttribute = property.GetCustomAttribute<MultipartFormDataFileAttribute>();
            if (multipartFormDataFileAttribute != null)
            {
                if (propValue is string path && File.Exists(path))
                {
                    var streamContent = new StreamContent(File.OpenRead(path));
                    formData.Add(streamContent, multipartKey, Path.GetFileName(path));
                }
                continue;
            }
            
            string newPrefix = useSquareBrakets ? $"{prefix}[{propName}]":$"{prefix}{propName}";
            AddObjectToMultipartFormDataContent(propValue, formData, newPrefix,true);
        }
    }
    private bool IsSimpleType(object obj)
    {
        if (obj == null) return false;

        var type = obj.GetType();
        type = Nullable.GetUnderlyingType(type) ?? type;

        return type.IsPrimitive
               //|| type.IsEnum
               || type == typeof(string)
               || type == typeof(decimal)
               //|| type == typeof(DateTime)
               || type == typeof(Guid)
               //|| type == typeof(DateTimeOffset)
               //|| type == typeof(TimeSpan)
               ;
    }
    
    private void AddDictionaryToMultipartFormDataContent(IDictionary dict, MultipartFormDataContent formData, string prefix = "")
    {
        Debug.WriteLine("AddDictionaryToMultipartFormDataContent");

        if (dict == null)
            return;

        foreach (DictionaryEntry kvp in dict)
        {
            var key = kvp.Key?.ToString() ?? string.Empty;
            var value = kvp.Value ?? string.Empty;
            var newPrefix = $"{prefix}[{key}]";
            AddObjectToMultipartFormDataContent(value, formData, newPrefix, true);
        }
    }
    /*
    private void AddDictionaryToMultipartFormDataContent(Dictionary<string, object> dict, MultipartFormDataContent formData,  string prefix = "")
    {
        Debug.WriteLine("AddDictionaryToMultipartFormDataContent");
        
        if (dict is not Dictionary<string, object> )
            return;
        
        foreach (var kvp in dict)
        {
            var key = kvp.Key;
            var value = kvp.Value ?? string.Empty;
            var newPrefix = $"{prefix}[{key}]";
            AddObjectToMultipartFormDataContent(value, formData, newPrefix, true);
        }
    }
    
    private void AddDictionaryToMultipartFormDataContent(Dictionary<string, string> dict, MultipartFormDataContent formData,  string prefix = "")
    {
        Debug.WriteLine("AddDictionaryToMultipartFormDataContent");
        
        if (dict is not Dictionary<string, string> )
            return;
        
        foreach (var kvp in dict)
        {
            var key = kvp.Key;
            var value = kvp.Value ?? string.Empty;
            var newPrefix = $"{prefix}[{key}]";
            AddObjectToMultipartFormDataContent(value, formData, newPrefix, true);
        }
    }
    */
    private void AddListToMultipartFormDataContent(List<object> list, MultipartFormDataContent formData,  string prefix = "")
    {
        Debug.WriteLine("AddListToMultipartFormDataContent");
        
        if (list is not List<object> )
            return;
            
        Dictionary<string, object> dict = list
            .Select((item, index) => new KeyValuePair<string, object>(index.ToString(), item))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        AddDictionaryToMultipartFormDataContent(dict, formData, prefix);
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
    private bool IsList(object obj)
    {
        if (obj == null) return false;

        var type = obj.GetType();
        return typeof(System.Collections.IList).IsAssignableFrom(type);
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