namespace MinimalAPI.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Returns the current <see cref="HttpContext"/> or throws when not available.
    /// Use only during an active HTTP request; do not capture or use this on background threads.
    /// </summary>
    public static HttpContext GetRequiredHttpContext(this IHttpContextAccessor? httpContextAccessor)
    {
        if (httpContextAccessor is null)
            throw new ArgumentNullException(nameof(httpContextAccessor));

        var httpContext = httpContextAccessor.HttpContext;

        if (httpContext == null) 
            throw new InvalidOperationException("HttpContext is not available for this operation.");

        return httpContext;
    }
}
