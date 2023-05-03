using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Akavache;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using KavaupMaui.Auth.Interfaces;
using KavaupMaui.Constant;
using KavaupMaui.Models;
using KavaupMaui.Providers.Interfaces;

namespace KavaupMaui.Auth;

public class AuthService : IAuthService
{
  private readonly OidcClient oidcClient;
  private readonly static IBlobCache _cache = BlobCache.Secure;

  public AuthService(AuthClientOptions options)
  {

    
    oidcClient = new OidcClient(new OidcClientOptions{
    Authority = $"https://{options.Domain}",
    ClientId = options.ClientId,
    Scope = options.Scope,
    RedirectUri = options.RedirectUri,
    Browser = options.Browser
    });
  }

  public IdentityModel.OidcClient.Browser.IBrowser Browser {
    get => oidcClient.Options.Browser;
    set => oidcClient.Options.Browser = value;
  }

  public async Task<LoginResult> LoginAsync()
  {
    var result = await oidcClient.LoginAsync();
    if (result.AccessToken == null) return result;
    SaveSession(new SessionKeys{ AccessToken = result.AccessToken, RefreshToken = result.RefreshToken,
    Expire = result.AccessTokenExpiration, UserName = result.User?.Identity?.Name});

    return result;
  }
  public async Task<BrowserResult> LogoutAsync() {
    var logoutParameters = new Dictionary<string,string>
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
  public async Task<SessionKeys> GetSession()
  {
    var obj = await _cache.GetObject<SessionKeys>("SessionKeys");
    return obj ?? null;
  }
  public async void SaveSession(SessionKeys sessionKeys)
  {
    await _cache.InsertObject<SessionKeys>("SessionKeys", sessionKeys, sessionKeys.Expire);//TODO ADD CORRECT INFORMATION HERE
  }
  public void ClearSession()
  {
    _cache.InvalidateAll().ToTask().Wait();
  }
  public async Task<SessionKeys> RefreshToken()
  {
    var obj = await _cache.GetObject<SessionKeys>("SessionKeys");
    if (obj == null) return null;//TODO PUSH TO MAIN PAGE
    var refresh = await oidcClient.RefreshTokenAsync(obj.RefreshToken);
    var newToken = new SessionKeys {
    AccessToken = refresh.AccessToken, RefreshToken = refresh.RefreshToken,
    Expire = refresh.AccessTokenExpiration, UserName = obj.UserName
    };
    SaveSession(newToken);
    return newToken;
  }
}
