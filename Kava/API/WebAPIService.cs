using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Kava.API;
// using Microsoft.Maui.Networking;
using Newtonsoft.Json;
using JsonException = System.Text.Json.JsonException;

namespace Kava.API;

public class WebAPIService : IWebAPIService
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ISessionManager _sessionManager;
        private readonly HttpClient _client;
        private readonly List<IWebAPIEndpoint> endpointQueue = new List<IWebAPIEndpoint>();
        private bool _isRefreshingToken;
        private const string AccessToken = "Access-Token";
        public WebAPIService(ISessionManager sessionManager,
                           IAuthenticationService authenticationService) {
            _authenticationService = authenticationService;
            _sessionManager = sessionManager;
            _client = new HttpClient();
            _client.Timeout = TimeSpan.FromSeconds(10);
        }
        public async Task<T> MakeRequest<T>(IWebAPIEndpoint endpoint, CancellationToken cancellationToken) where T : class
        {
            try
            {
            //Check if the device is connected to the internet before attempting to send the request
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                throw new Exception($"No internet connection: {Connectivity.NetworkAccess}");
            }
            var httpResponse = await HttpRequest(endpoint, _client, cancellationToken);
                if (httpResponse == null) return default(T);
                var responseString = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    StatusCodeCheck(httpResponse, responseString);
                }
                catch (Exception ex)//TODO WHAT DOES THIS DO?//UnAuthorizedException exception)
                {
                    switch (_isRefreshingToken)
                    {
                        case false:
                            await RefreshAuthenticationToken(cancellationToken);
                            break;
                        case true:
                            return await QueueWhileTokenIsRefreshing<T>(endpoint, cancellationToken);
                        default:
                            InvalidateSessionAndPromptToSignIn();
                            return default(T);
                    }
                    endpointQueue.Remove(endpoint);
                    return await MakeRequest<T>(endpoint, cancellationToken);
                }
                return DeserializeJson<T>(responseString);
            }
            catch (Exception ex)
            {
            //TODO CREATE LOG MANAGER
               // Logger.Write(ex);
                throw;
            }
        }
        private async Task<T> QueueWhileTokenIsRefreshing<T>(IWebAPIEndpoint endpoint, CancellationToken cancellationToken) where T : class
        {
            endpointQueue.Add(endpoint);
            return await MakeRequest<T>(endpoint, cancellationToken);
        }
        private async Task RefreshAuthenticationToken(CancellationToken cancellationToken)
        {
            var refreshToken = _sessionManager.GetSession().RefreshToken;
            switch (_isRefreshingToken)
            {
                case false when !string.IsNullOrEmpty(refreshToken):
                    _isRefreshingToken = true;
                    try
                    {
                        var authToken = await _authenticationService.RefreshToken(
                            _sessionManager.GetSession());
                        _isRefreshingToken = false;
                        _sessionManager.SaveSession(authToken);
                    }
                    catch (Exception ex)
                    {
                        InvalidateSessionAndPromptToSignIn();
                    }
                    break;
                case true:
                    InvalidateSessionAndPromptToSignIn();
                    break;
                default:
                    {
                        if (string.IsNullOrEmpty(refreshToken))
                        {
                            InvalidateSessionAndPromptToSignIn();
                        }
                        break;
                    }
            }
        }
        private void InvalidateSessionAndPromptToSignIn()
        {
            _sessionManager.ClearSession();
            //TODO:logout and go to sign in page
        }
        private async Task<HttpResponseMessage> HttpRequest(IWebAPIEndpoint endpoint, HttpClient client, CancellationToken cancellationToken)
        {
            AddHeaders(client, endpoint);
            var url = endpoint.DomainUrl + endpoint.Url;
            if (endpoint.HttpVerb == HttpVerb.Get)
                return await MakeHttpRequest(endpoint, client, url, cancellationToken);
                return await MakeHttpRequest(endpoint, client, url, cancellationToken, endpoint.Payload);
                }
        private void AddHeaders(HttpClient client, IWebAPIEndpoint endpoint)
        {
            if (!string.IsNullOrEmpty(_sessionManager.GetSession()?.IdToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_sessionManager.GetSession().IdToken);
                if (client.DefaultRequestHeaders.Contains(AccessToken))
                {
                    client.DefaultRequestHeaders.Remove(AccessToken);
                }
                //set latest every time
                client.DefaultRequestHeaders.Add(AccessToken, _sessionManager.GetSession().AccessToken);
            }
            if (null == endpoint.RequestHeader) return;
            foreach (var header in endpoint.RequestHeader)
            {
                switch (header.Key)
                {
                    case HeaderTypes.ContentType:
                        client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                        break;
                    case HeaderTypes.Accept:
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(header.Value));
                        break;
                    case HeaderTypes.Authorization:
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(header.Key, header.Value);
                        break;
                    default:
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        break;
                }
            }
        }
        private async Task<HttpResponseMessage> MakeHttpRequest(IWebAPIEndpoint endpoint,
                                                               HttpClient client,
                                                               string url,
                                                               CancellationToken cancellationToken,
                                                               object? payload = null)
        {
            HttpResponseMessage result = null;
            try
            {
                switch (endpoint.HttpVerb)
                {
                    case HttpVerb.Delete:
                        //TODO:implement
                        break;
                    case HttpVerb.Get:
                        result = await client.GetAsync(url, cancellationToken);
                        break;
                    case HttpVerb.Patch:
                        //TODO:implement
                        break;
                    case HttpVerb.Post:
                        {
                            result = await client.PostAsync(url, WebAPIService.SerializeJson(payload), cancellationToken);
                        }
                        break;
                    case HttpVerb.Put:
                        //TODO:implement
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (TaskCanceledException ex)
            {
                //If the task was not cancelled due to user navigation, throw a timeout exception
                if (!ex.CancellationToken.IsCancellationRequested)
                {
                    throw new Exception("web timeout");
                }
                else throw;
            }
            catch (Exception e)
            {
                throw;
            }
            return result;
        }
        private static HttpContent SerializeJson(object payload)
        {
            if (payload == null)
            {
                return null;
            }
            var data = JsonConvert.SerializeObject(payload);
            var content = new StringContent(data, Encoding.UTF8, MediaTypes.Json);
            return content;
        }
        private void StatusCodeCheck(HttpResponseMessage result, string responseString)
        {
            switch (result.StatusCode)
            {
                case HttpStatusCode.Unauthorized:
                    //TODO WHAT DOES THIS DO?throw new UnAuthorizedException();
                case HttpStatusCode.NotFound:
                case HttpStatusCode.BadRequest:
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.Forbidden:
                    throw new Exception(result.StatusCode.ToString());
                case HttpStatusCode.OK:
                    break;
                default:
                    Console.WriteLine("issue");
                    break;
            }
        }
        private static T DeserializeJson<T>(string response)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(response);
            }
            catch (JsonException ex)
            {
                throw new JsonException("Could not parse JSON.", ex);
            }
        }
    }
internal static class HeaderTypes
{
    public const string ContentType = "ContentType";
    public const string Accept = "Accept"; 
    public const string Authorization = "Authorization";
}
internal static class MediaTypes
{
    public static string Json { get => "application/json"; }
}
