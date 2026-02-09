using MinimalAPI.Enums;

namespace MinimalAPI.Domain.DTOs;

public record ReturnResult<T>
(
    bool Success,
    string Message,
    T? Data,
    AuthErrorType ErrorType = AuthErrorType.UnknownError
)
{
    public static ReturnResult<T> Ok(T data)
        => new(true, "", data);
    public static ReturnResult<T> Fail(AuthErrorType errorType, string message = "")
        => new(false, message, default, errorType);
}

public record ReturnAuthResult<T>
(
    bool Success,
    string Message,
    T? Data,
    AuthErrorType ErrorType,
    TokenResponseDTO? TokenResponse
)
{
    public static ReturnAuthResult<T> Ok(T data, TokenResponseDTO? token = default)
        => new(true, "", data, AuthErrorType.NoneError, token);
    public static ReturnAuthResult<T> Fail(AuthErrorType errorType, string message = "")
        => new(true, message, default, errorType, default);
}
