using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Channels;
using Akavache;
using Kava.API.Interfaces;
using Kava.Models;
using Kava.Providers.Interfaces;

namespace Kava.API;

public class NetworkSessionManager : INetworkSessionManager
{
  private readonly static IBlobCache _cache = BlobCache.Secure;
  private ICacheProvider _cacheProvider;
  public async Task<SessionKeys> GetSession()
  {
    var obj = await _cache.GetObject<SessionKeys>("SessionKeys");
    return obj ?? null;
  }
  public async void SaveSession(string authToken)
  {
    await _cache.InsertObject("auth_token", authToken, TimeSpan.FromDays(1));//TODO ADD CORRECT INFORMATION HERE
  }
  public void ClearSession()
  {
    _cache.InvalidateAll().ToTask().Wait();
  }
}
