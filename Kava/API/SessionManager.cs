using Kava.Models;
using Kava.Storage;

namespace Kava.API;

public class SessionManager : ISessionManager
{
    private readonly ICacheProvider _cacheProvider;

    public SessionManager(ICacheProvider cacheProvider)
    {
        _cacheProvider = cacheProvider;
    }

    public Session GetSession()
    {
        return _cacheProvider.GetObject<Session>("Session");
    }
    public void SaveSession(Session session)
    {
        _cacheProvider.InsertObject("Session", session);//TODO ADD CORRECT INFORMATION HERE
    }
    public void ClearSession()
    {
        _cacheProvider.DeleteAll();
    }
}
