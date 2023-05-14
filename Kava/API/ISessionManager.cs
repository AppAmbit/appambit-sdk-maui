using Kava.Models;

namespace Kava.API;

public interface ISessionManager
{
    Session GetSession();
    void SaveSession(Session session);
    void ClearSession();
}
