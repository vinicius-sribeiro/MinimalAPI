using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Extensions;
using System.Security.Cryptography;

namespace MinimalAPI.Domain.Services;

public class HttpContextService : IHttpContext
{
    private readonly IHttpContextAccessor _httpContextAcessor;

    public HttpContextService(IHttpContextAccessor httpContextAcessor)
    {
        _httpContextAcessor = httpContextAcessor;
    }

    public void GerenateCookie(string key, string jwt, CookieOptions cookieOptions)
    {
        _httpContextAcessor.GetRequiredHttpContext().Response.Cookies.Append(
            key,
            jwt,
            cookieOptions
        );
    }

    public string GenerateCsrfToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
