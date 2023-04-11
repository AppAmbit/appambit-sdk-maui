using KavaupMaui.Models;

namespace KavaupMaui.API.Interfaces;

public interface INetworkSessionManager
{
  Task<SessionKeys> GetSession();
  void SaveSession(string authToken);
  void ClearSession();
}
