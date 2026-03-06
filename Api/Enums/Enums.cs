namespace MinimalAPI.Enums;

public enum Cargo
{
    Admin = 1,
    User = 2
}

public enum AuthErrorType
{
    InvalidCredentials,
    Unauthorized,
    UserNotFound,
    EmailAlreadyExists,
    InactiveAccount,
    UnknownError,
    NoneError
}