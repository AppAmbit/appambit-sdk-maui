using IdentityModel.OidcClient;
using KavaupMaui.Constant;

namespace KavaupMaui.Auth;

public class AuthClient
{
  private readonly OidcClient oidcClient;
  
  public AuthClient(AuthClientOptions options)
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
   
    return await oidcClient.LoginAsync();
  }
}
