using System;
namespace Kava.API
{
    public class NetworkException : Exception
    {
        public const string GenericErrorText = "Something went wrong...";

        public NetworkException(string message = GenericErrorText) : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown on 400 error code.
    /// </summary>
    public class BadRequestException : NetworkException
    {
        public BadRequestException(string message = GenericErrorText) : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown on 404 error code.
    /// </summary>
    public class ResourceNotFoundException : NetworkException
    {
        public ResourceNotFoundException(string message = GenericErrorText) : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown on 401 error code.
    /// </summary>
    public class UnauthorizedException : NetworkException
    {
        public const string UnauthorizedErrorText = "Unauthorized";

        public UnauthorizedException(string message = UnauthorizedErrorText) : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown on 404 error code.
    /// </summary>
    public class RequestForbiddenException : NetworkException
    {
        public const string ForbiddendErrorText = "Forbidden";

        public RequestForbiddenException(string message = ForbiddendErrorText) : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown on 500 error code.
    /// </summary>
    public class InternalServerException : NetworkException
    {
        public InternalServerException(string message = GenericErrorText) : base(message)
        {
        }
    }
    /// <summary>
    /// Thrown on 408 error code.
    /// </summary>
    public class RequestTimeoutException : NetworkException
    {
        public RequestTimeoutException(string message = "Connection timed out.") : base(message)
        {
        }
    }

    /// <summary>
    /// Thrown on 401 error code after an attempt to refresh token.
    /// </summary>
    public class RefreshTokenFailedException : NetworkException
    {
        public RefreshTokenFailedException(string message = "Session expired. Please sign in again.") : base(message)
        {
        }
    }
}

