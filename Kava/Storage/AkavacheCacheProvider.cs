using System.Reactive.Threading.Tasks;
using Akavache;

namespace Kava.Storage;

public class AkavacheCacheProvider : ICacheProvider
{
    public static string AppName = "KavaUp.MAUI";
    readonly IBlobCache cache;

    public AkavacheCacheProvider()
    {
        BlobCache.ApplicationName = AppName;
        BlobCache.ForcedDateTimeKind = DateTimeKind.Local;
        this.cache = BlobCache.Secure;
    }

    public void InsertObject<T>(string key, T value, DateTimeOffset? absoluteExpiration = null)
    {
        cache.InsertObject(key, value, absoluteExpiration).ToTask().Wait();
    }

    public void DeleteAll()
    {
        cache.InvalidateAll().ToTask().Wait();
    }

    public void Delete<T>(string key)
    {
        cache.InvalidateObject<T>(key).ToTask().Wait();
    }

    public bool Exists(string key)
    {
        bool exists = true;
        try
        {
            var result = cache.Get(key).ToTask().Result;
        }
        catch (AggregateException ex)
        {
            if (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is KeyNotFoundException)
            {
                exists = false;
            }
        }

        return exists;
    }

    public T GetObject<T>(string key)
    {
        try
        {
            return cache.GetObject<T>(key).ToTask().Result;
        }
        catch (AggregateException ex)
        {
            if (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is KeyNotFoundException)
            {
                return default(T);
            }
            throw ex;
        }
    }

    public Task<T> GetOrFetchObject<T>(string key, Func<Task<T>> fetchFunc)
    {
        return cache.GetOrFetchObject(key, fetchFunc).ToTask();
    }

}