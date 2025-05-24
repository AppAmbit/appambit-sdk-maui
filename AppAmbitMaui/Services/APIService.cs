using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppAmbit.Models.Logs;
using AppAmbit.Services.Endpoints;
using AppAmbit.Services.Interfaces;
using Newtonsoft.Json;
namespace AppAmbit.Services
{
    /// <summary>
    /// DelegatingHandler que intercepta todas las peticiones y respuestas para loggear en consola.
    /// </summary>
    public class LoggingHandler : DelegatingHandler
    {
        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler)
        { }
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Log de la petición
            Console.WriteLine("----- HTTP REQUEST -----");
            Console.WriteLine($"{request.Method} {request.RequestUri}");
            Console.WriteLine(request.Headers);
            if (request.Content != null)
            {
                var reqBody = await request.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine(reqBody);
            }
            // Ejecuta la petición real
            var response = await base.SendAsync(request, cancellationToken);
            // Log de la respuesta
            Console.WriteLine("----- HTTP RESPONSE -----");
            Console.WriteLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
            Console.WriteLine(response.Headers);
            if (response.Content != null)
            {
                var respBody = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine(respBody);
            }
            return response;
        }
    }
    internal class APIService : IAPIService
    {
        private string? _token;
        public async Task<T?> ExecuteRequest<T>(IEndpoint endpoint) where T : notnull
        {
            try
            {
                // Usar LoggingHandler para capturar en consola todas las peticiones y respuestas
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
                Debug.WriteLine($"StatusCode:{(int)responseMessage.StatusCode} {responseMessage.StatusCode}");
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                Debug.WriteLine($"responseString:{responseString}");
                return TryDeserializeJson<T>(responseString);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception:{e.Message}");
                return default(T);
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
                return JsonConvert.DeserializeObject<T>(response)!;
            }
            catch (JsonException)
            {
                throw new JsonException("Could not parse JSON. Something went wrong.");
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
        private async Task<HttpContent> SerializePayload(object payload)
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
            }
            else if (payload is LogBatch logBatch)
            {
                content = SerializeToMultipartFormDataContent(logBatch);
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
            return new StringContent(data, Encoding.UTF8, "application/json");
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
                    + Uri.EscapeDataString((string)pi.GetValue(payload))
                    + "&");
            return serializedPayload.TrimEnd('&');
        }
        private string SerializedGetURL(string url, object payload)
        {
            var serializedParameters = SerializeStringPayload(payload);
            if (string.IsNullOrEmpty(serializedParameters))
            {
                return url;
            }
            return url + "?" + serializedParameters;
        }
        private async Task<HttpResponseMessage> GetHttpResponseMessage(IEndpoint endpoint, HttpClient client, string url, object payload)
        {
            try
            {
                return endpoint.Method switch
                {
                    HttpMethodEnum.Get => await client.GetAsync(SerializedGetURL(url, payload)),
                    HttpMethodEnum.Post => await client.PostAsync(url, await SerializePayload(payload)),
                    HttpMethodEnum.Patch => await client.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), url)
                    {
                        Content = await SerializePayload(payload)
                    }),
                    HttpMethodEnum.Put => await client.PutAsync(url, await SerializePayload(payload)),
                    HttpMethodEnum.Delete => await client.DeleteAsync(url),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Request timed out.");
            }
        }
    }
}