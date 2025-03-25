using Shared.Models.Endpoints.Base;

namespace iOSAppAmbit.Services.Base;

public interface IAPIService
{
    Task<T> ExecuteRequest<T>(IEndpoint endpoint);

}