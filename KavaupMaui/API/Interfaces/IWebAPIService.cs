namespace KavaupMaui.API.Interfaces;

public interface IWebAPIService
{
  // Task<T> MakeRequest<T>(IWebAPIEndpoint endpoint, CancellationToken cancellationToken) where T : class;
  Task<TResult> GetAsync<TResult>(string uri);

  Task<TResult> PostAsync<TResult>(string uri, TResult data);

  Task<TResult> PostAsync<TResult>(string uri, string data);

  Task<TResult> PutAsync<TResult>(string uri, TResult data);

  Task DeleteAsync(string uri);

  
}
