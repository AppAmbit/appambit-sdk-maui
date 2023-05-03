using IdentityModel.OidcClient;
using IdentityModel.OidcClient.Browser;
using KavaupMaui.Models;

namespace KavaupMaui.Auth.Interfaces;

public interface IAuthService
{
  Task<LoginResult> LoginAsync();

  Task<BrowserResult> LogoutAsync();
  Task<SessionKeys> GetSession();
  void SaveSession(SessionKeys sessionKeys);
  void ClearSession();

  Task<SessionKeys> RefreshToken();
}
