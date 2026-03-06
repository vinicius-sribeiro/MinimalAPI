using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.Models;

namespace MinimalAPI.Infrastructure;

public class MinimalApiContext : DbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();

    public MinimalApiContext(DbContextOptions<MinimalApiContext> options) : base(options)
    {
    }

    public MinimalApiContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseMySql("name=ConnectionStrings:DefaultConnection", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.45-mysql"));
        }
    }   
}

