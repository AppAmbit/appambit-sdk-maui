using Kava.Models;

namespace Kava.API.Interfaces;

public interface IAuthenticationService
{
  Task<string> RefreshToken(SessionKeys sessionKeys);
}
