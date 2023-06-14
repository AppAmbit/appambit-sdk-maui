using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Kava.API;
using Kava.Models;

namespace Kava.Oauth;

public interface IOAuthService : ITokenRefreshService
{
    Task<LoginResult> LoginAsync();
    Task<BrowserResult> LogoutAsync();

    Session GetSession();
    void SaveSession(Session session);
    void ClearSession();
}
