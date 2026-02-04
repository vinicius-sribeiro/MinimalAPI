namespace MinimalAPI.Enums;

public enum Cargo
{
    Admin = 1,
    User = 2
}

public enum  StatusCode
{
    Ok = 200,
    Created = 201,
    NoContent = 204,
    BadRequest = 400,
    Unauthorized = 401,
    Forbidden = 403,
    NotFound = 204,
    Conflict = 409
}
