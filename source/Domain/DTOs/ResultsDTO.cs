using MinimalAPI.Enums;

namespace MinimalAPI.Domain.DTOs;

public record ReturnResult<T>
(
    bool Success,
    string Message,
    T? Data,
    StatusCode StatusCode = StatusCode.Ok
)
{
    public static ReturnResult<T> Ok(T data)
        => new(true, "", data);

    public static ReturnResult<T> Fail(StatusCode status, string message)
        => new(false, message, default, status);
}
