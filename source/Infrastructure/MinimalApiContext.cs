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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("name=ConnectionStrings:DefaultConnection", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.45-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = -1,
                Nome = "Admin Teste",
                Email = "admin@teste.com",
                SenhaHash = "123456",
                Cargo = Enums.Cargo.Admin,
                isAtivo = true,
                DataCriacao = new DateTime(2026, 1, 1)
            }
        );
    }
}

