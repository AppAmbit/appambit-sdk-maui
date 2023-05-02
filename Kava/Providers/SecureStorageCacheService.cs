// using Microsoft.Maui.Storage;
using Newtonsoft.Json;

namespace Kava.Providers;

public class SecureStorageCacheService
{
  public void Insert<T>(string key, T cacheObject)
  {
    var serializedValue = JsonConvert.SerializeObject(cacheObject);
    // SecureStorage.SetAsync(key, serializedValue).GetAwaiter().GetResult();//TODO ADD BACK IN
  }

  public T Get<T>(string key)
  {
    // var storedValue = SecureStorage.GetAsync(key)?.GetAwaiter().GetResult();
    var storedValue = "{}";//TODO ADD IN
    return !string.IsNullOrEmpty(storedValue) ? JsonConvert.DeserializeObject<T>(storedValue) : default;
  }
}
