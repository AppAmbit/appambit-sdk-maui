namespace AppAmbit.Services.Interfaces;

internal interface IAPIService
{ 
    Task<T> ExecuteRequest<T>(IEndpoint endpoint);
}