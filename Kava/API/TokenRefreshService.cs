using Kava.API;
using Kava.Models;

namespace Kava.API;

public class TokenRefreshService : ITokenRefreshService
{
  public async Task<Session> RefreshToken(Session sessionKeys)
  {
    //TODO Call out to get auth
    return new Session();
  }
}
