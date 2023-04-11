using KavaupMaui.API.Interfaces;
using KavaupMaui.Models;

namespace KavaupMaui.API;

public class AuthenticationService : IAuthenticationService
{
  public async Task<string> RefreshToken(SessionKeys sessionKeys)
  {
    //TODO Call out to get auth
    return "Auth";
  }
}
