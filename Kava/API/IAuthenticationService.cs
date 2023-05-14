using Kava.Models;

namespace Kava.API;

public interface IAuthenticationService
{
  Task<Session> RefreshToken(Session sessionKeys);
}
