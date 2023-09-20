using Newtonsoft.Json;

namespace Kava.Storage;

public class SecureStorageCacheService
{
    public void Insert<T>(string key, T cacheObject)
    {
        var serializedValue = JsonConvert.SerializeObject(cacheObject);
        SecureStorage.SetAsync(key, serializedValue).GetAwaiter().GetResult();
    }

    public T Get<T>(string key)
    {
        var storedValue = SecureStorage.GetAsync(key)?.GetAwaiter().GetResult();
        return !string.IsNullOrEmpty(storedValue) ? JsonConvert.DeserializeObject<T>(storedValue) : default;
    }
}
