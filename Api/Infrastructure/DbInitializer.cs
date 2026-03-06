using MinimalAPI.Domain.Models;

namespace MinimalAPI.Infrastructure
{
    public class DbInitializer
    {
        public static void Seed(MinimalApiContext context)
        {
            if (!context.Usuarios.Any())
            {
                context.Usuarios.Add(new Usuario {
                    Id = -1,
                    Nome = "Admin Teste",
                    Email = "admin@teste.com",
                    SenhaHash = "123456",
                    Cargo = Enums.Cargo.Admin,
                    isAtivo = true,
                    DataCriacao = new DateTime(2026, 1, 1)
                });
                context.SaveChanges();
            }
        }
    }
}
