namespace MinimalAPI.Domain.Interfaces;

public interface IHttpContext
{
    public void GerenateCookie(string key, string jwt, CookieOptions cookieOptions);
    public string GenerateCsrfToken();
}
