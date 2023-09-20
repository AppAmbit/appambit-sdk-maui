using Kava.Models;

namespace Kava.API;

public class AuthenticationService : IAuthenticationService
{
  public async Task<Session> RefreshToken(Session sessionKeys)
  {
    //TODO Call out to get auth
    return new Session();
  }
}
