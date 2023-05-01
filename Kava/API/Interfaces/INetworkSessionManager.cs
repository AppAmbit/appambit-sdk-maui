using Kava.Models;

namespace Kava.API.Interfaces;

public interface INetworkSessionManager
{
  Task<SessionKeys> GetSession();
  void SaveSession(string authToken);
  void ClearSession();
}
