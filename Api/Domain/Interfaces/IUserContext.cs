using MinimalAPI.Enums;
using System.Security.Claims;

namespace MinimalAPI.Domain.Interfaces;

public interface IUserContext
{
    /// <summary>
    /// Underlying principal (may be null outside an HTTP request).
    /// </summary>
    ClaimsPrincipal? User { get; }

    /// <summary>
    /// True when the user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Nullable integer user id parsed from common claims (NameIdentifier, sub, "id").
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// User email if present in claims.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Display name if present in claims.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Tries to get the user id.
    /// </summary>
    bool TryGetUserId(out int userId);

    /// <summary>
    /// Checks role membership on the current principal.
    /// </summary>
    bool IsInRole(Cargo role);
}
