using Kava.Models;

namespace Kava.API;

public interface ITokenRefreshService
{
  Task<Session> RefreshToken(Session sessionKeys);
}
