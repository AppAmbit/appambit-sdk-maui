using AppAmbit.Enums;
namespace AppAmbit.Models.Responses;

public class ApiResult<T>
{
    public T? Data { get; }
    public ApiErrorType ErrorType { get; }
    public string? Message { get; }

    public ApiResult(T? data, ApiErrorType errorType, string? message = null)
    {
        Data = data;
        ErrorType = errorType;
        Message = message;
    }

    public static ApiResult<T> Success(T data) => new(data, ApiErrorType.None);
    public static ApiResult<T> Fail(ApiErrorType error, string? message = null) => new(default, error, message);    
}

