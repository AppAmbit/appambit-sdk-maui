using KavaupMaui.Models;

namespace KavaupMaui.API.Interfaces;

public interface IAuthenticationService
{
  Task<string> RefreshToken(SessionKeys sessionKeys);
}
