using Microsoft.IdentityModel.Tokens;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MinimalAPI.Domain.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(Usuario user)
    {
        // Pegar as configurações do appsettings.json (user-secrets)
        var secretKey = _config["Jwt:Key"]!;
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var expirationInMinutes = int.Parse(_config["Jwt:ExpirationInMinutes"]!);

        // Criar chave de segurança
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Definir CLAIMS (dados do usuário no token)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),            
            new Claim(ClaimTypes.Role, user.Cargo.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique identifier for the token
        };

        // Criar Token 
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationInMinutes),
            signingCredentials: credentials
        );

        // Gerar string do token
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public DateTime ObterDataExpiracao()
    {
        var expirationMinutes = int.Parse(_config["Jwt:ExpirationInMinutes"]!);
        return DateTime.UtcNow.AddMinutes(expirationMinutes);
    }    
}
