namespace KavaupMaui.Providers.Interfaces;

public interface ICacheProvider 
{
 // void Insertobjects<T>(string key, T value, DateTimeOffset? absoluteExpiration = null);
 void InsertObject<T>(string key, T value, DateTimeOffset? absoluteExpiration = null);
  void DeleteAll();
  void Delete<T>(string key);
  bool Exists (string key);
  T GetObject<T>(string key);
  Task<T> GetOrFetchObject<T>(string key, Func<Task<T>> fetchFunc);
}
