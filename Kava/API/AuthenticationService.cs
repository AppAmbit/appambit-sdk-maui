using Kava.API.Interfaces;
using Kava.Models;

namespace Kava.API;

public class AuthenticationService : IAuthenticationService
{
  public async Task<string> RefreshToken(SessionKeys sessionKeys)
  {
    //TODO Call out to get auth
    return "Auth";
  }
}
