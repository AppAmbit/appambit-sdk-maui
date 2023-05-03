using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KavaupMaui.API.Interfaces;
using KavaupMaui.Auth.Interfaces;
using KavaupMaui.Helpers.DialogResults;

namespace KavaupMaui.API;

public class WebAPIService : IWebAPIService
{
    private readonly IAuthService _authService;
    private readonly HttpClient _client;
    private readonly List<IWebAPIEndpoint> endpointQueue = new List<IWebAPIEndpoint>();
    private readonly IDialogResults _dialogResults;
    private bool _isRefreshingToken;
    private readonly HttpClient _httpClient = new HttpClient {
    Timeout = new TimeSpan(100000), DefaultRequestHeaders = {
    Accept = { new MediaTypeWithQualityHeaderValue("application/json") } } };
    public WebAPIService(IAuthService sessionManager, IDialogResults dialogResults
    )
    {
        _authService = sessionManager;
        _dialogResults = dialogResults;
        var accessToken = _authService.GetSession().Result.AccessToken;
        _httpClient.DefaultRequestHeaders.Authorization =
        !string.IsNullOrEmpty(accessToken) 
        ? new AuthenticationHeaderValue("Bearer", accessToken) 
        : null;
    }
      public async Task<TResult> GetAsync<TResult>(string uri)
    {
        var response = await _httpClient.GetAsync(uri).ConfigureAwait(false);

        await HandleResponse(response).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<TResult>();

        return result;
    }

    public async Task<TResult> PostAsync<TResult>(string uri, TResult data)
    {

        var content = new StringContent(JsonSerializer.Serialize(data));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await _httpClient.PostAsync(uri, content).ConfigureAwait(false);

        await HandleResponse(response).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<TResult>();

        return result;
    }

    public async Task<TResult> PostAsync<TResult>(string uri, string data)
    {
        var content = new StringContent(data);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
        var response = await _httpClient.PostAsync(uri, content).ConfigureAwait(false);

        await HandleResponse(response).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<TResult>();

        return result;
    }

    public async Task<TResult> PutAsync<TResult>(string uri, TResult data)
    {

        var content = new StringContent(JsonSerializer.Serialize(data));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await _httpClient.PutAsync(uri, content).ConfigureAwait(false);

        await HandleResponse(response).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<TResult>();

        return result;
    }

    public async Task DeleteAsync(string uri)
    {
        
        await _httpClient.DeleteAsync(uri).ConfigureAwait(false);
    }
    private static void AddHeaderParameter(HttpClient httpClient, string parameter)
    {
        if (httpClient == null)
            return;
        if (string.IsNullOrEmpty(parameter))
            return;

        httpClient.DefaultRequestHeaders.Add(parameter, Guid.NewGuid().ToString());
    }

    private async Task HandleResponse(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            // var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Forbidden ||
                    response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var refreshed = await _authService.RefreshToken();
                if (refreshed == null)
                {
                    var login = _authService.LoginAsync();
                    if (login != null)
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Result.AccessToken);
                        await _dialogResults.ShowAlertAsync("Login", "Successfully Logged In!", "Ok");
                    }
                }
                else
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshed.AccessToken);
                    //TODO HANDLE SUCCESS REFRESH?
                }
            }

            //TODO CHECK if we want to do a dialog box?
        }
        
    }
}