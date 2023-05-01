using Akavache;
using Kava.Helpers.AppName;
using Kava.Providers.Interfaces;

namespace Kava.Providers;

public class AkavacheCacheManager //: BaseContainerModule, ICacheManager
{
  const string AppCacheFilename = "NEWMAUI_APPID.sqlite3";

  public AkavacheCacheManager()
  {
    BlobCache.ApplicationName = ApplicationName.AppName;
    BlobCache.ForcedDateTimeKind = DateTimeKind.Local;

    AppCache = new AkavacheCacheProvider(BlobCache.Secure);
  }

  #region ICacheManager

  public ICacheProvider AppCache { get; private set; }

  // public override void Load()
  // {
  // }

  #endregion
}