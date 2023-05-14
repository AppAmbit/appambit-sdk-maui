using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using Kava.Models;

namespace Kava.Oauth;

public interface IOAuthService
{
  Task<LoginResult> LoginAsync();
  Task<BrowserResult> LogoutAsync();

  Session GetSession();
  void SaveSession(Session session);
  void ClearSession();

  Task<Session> RefreshToken();
}
