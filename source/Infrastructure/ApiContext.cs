using Microsoft.EntityFrameworkCore;

namespace MinimalAPI.Infrastructure;

public class ApiContext : DbContext
{
    public ApiContext (DbContextOptions<ApiContext> options) : base (options)
    {
    }
}

