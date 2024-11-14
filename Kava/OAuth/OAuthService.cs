using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Kava.Models;
using Kava.Storage;

namespace Kava.Oauth;

public class OAuthService : IOAuthService
{
    private readonly OidcClient oidcClient;
    private ICacheProvider _cache;

    public OAuthService(OAuthClientOptions options, ICacheProvider cache)
    {
        _cache = cache;
        oidcClient = new OidcClient(new OidcClientOptions
        {
            Authority = $"https://{options.Domain}",
            ClientId = options.ClientId,
            Scope = options.Scope,
            RedirectUri = options.RedirectUri,
            Browser = options.Browser
        });
    }

    public IdentityModel.OidcClient.Browser.IBrowser Browser
    {
        get => oidcClient.Options.Browser;
        set => oidcClient.Options.Browser = value;
    }

    public async Task<LoginResult> LoginAsync()
    {
        var result = await oidcClient.LoginAsync();
        if (result.AccessToken == null) return result;
        SaveSession(new Session
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken,
            IdToken = result.IdentityToken,
            UserName = result.User?.Identity?.Name
        });

        return result;
    }
    public async Task<BrowserResult> LogoutAsync()
    {
        var logoutParameters = new Dictionary<string, string>
        {
            {"client_id", oidcClient.Options.ClientId },
            {"returnTo", oidcClient.Options.RedirectUri }
        };

        var logoutRequest = new LogoutRequest();
        var endSessionUrl = new RequestUrl($"{oidcClient.Options.Authority}/v2/logout")
            .Create(new Parameters(logoutParameters));
        var browserOptions = new BrowserOptions(endSessionUrl, oidcClient.Options.RedirectUri)
        {
            Timeout = TimeSpan.FromSeconds(logoutRequest.BrowserTimeout),
            DisplayMode = logoutRequest.BrowserDisplayMode
        };

        var browserResult = await oidcClient.Options.Browser.InvokeAsync(browserOptions);
        ClearSession();
        return browserResult;
    }
    
    public Session GetSession()
    {
        var obj = _cache.GetObject<Session>("Session");
        return obj ;
    }
    
    public void SaveSession(Session Session)
    {
        _cache.InsertObject<Session>("Session", Session, DateTime.Now.AddHours(24));//TODO ADD CORRECT INFORMATION HERE
    }
    
    public void ClearSession()
    {
        _cache.DeleteAll();
    }
    
    public async Task<Session> RefreshToken()
    {
        var obj = _cache.GetObject<Session>("Session");
        if (obj == null) return null;//TODO PUSH TO MAIN PAGE
        var refresh = await oidcClient.RefreshTokenAsync(obj.RefreshToken);
        var newToken = new Session
        {
            AccessToken = refresh.AccessToken,
            RefreshToken = refresh.RefreshToken,
            IdToken = refresh.IdentityToken,
            UserName = obj.UserName
        };
        SaveSession(newToken);
        return newToken;
    }
}