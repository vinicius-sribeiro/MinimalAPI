using MinimalAPI.Domain.Models;

namespace MinimalAPI.Domain.Interfaces;

public interface ITokenService
{
    string GenerateToken(Usuario user);
    public DateTime ObterDataExpiracao();
}
