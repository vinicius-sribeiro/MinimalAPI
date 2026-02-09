using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Enums;
using MinimalAPI.Extensions;
using System.Security.Claims;

namespace MinimalAPI.Domain.Services;

public class HttpContextUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAcessor;

    public HttpContextUserContext(IHttpContextAccessor httpContextAcessor)
    {
        _httpContextAcessor = httpContextAcessor;
    }

    public ClaimsPrincipal? User => _httpContextAcessor.GetRequiredHttpContext().User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public int? UserId => User?.GetUserId();

    public string? Email => User?.GetEmail();

    public string? Name => User?.GetUserName();

    public bool IsInRole(Cargo role) => User?.IsInRole(role.ToString()) ?? false;

    public bool TryGetUserId(out int userId)
    {
        userId = User?.GetUserId() ?? -2;        
        return userId >= -1;
    }
}
