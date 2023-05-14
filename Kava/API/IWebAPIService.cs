namespace Kava.API;

public interface IWebAPIService
{
  Task<T> MakeRequest<T>(IWebAPIEndpoint endpoint, CancellationToken cancellationToken) where T : class;
}
