using AppAmbitSdkCore.Enums;
using AppAmbitSdkCore.Models.Responses;

namespace AppAmbitSdkCore.Services.Interfaces;

internal interface IAPIService
{
    Task<ApiResult<T>?> ExecuteRequest<T>(IEndpoint endpoint) where T : notnull;

    void SetToken(string? token);

    string? GetToken();

    Task<ApiErrorType> GetNewToken();
    
}
