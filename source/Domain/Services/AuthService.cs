using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Models;
using MinimalAPI.Enums;
using MinimalAPI.Infrastructure;
using System.Linq.Expressions;

namespace MinimalAPI.Domain.Services;

public class AuthService : IAuthService
{
    public readonly MinimalApiContext _context;

    public AuthService(MinimalApiContext context)
    {
        _context = context;
    }

    public ReturnResult<Usuario> RegisterUser(RegisterDto dto, Cargo cargo)
    {
        // Validar se email já existe
        if (_context.Usuarios.Any(u => string.Equals(u.Email, dto.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return ReturnResult<Usuario>.Fail(StatusCode.Conflict, "Email já cadastrado");
        }       

        var newUser = new Usuario
        {
            Nome = dto.Nome,
            Email = dto.Email.ToLower(),
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Cargo = cargo,
            isAtivo = true,
            DataCriacao = DateTime.UtcNow
        };

        // Gerar Token

        _context.Usuarios.Add(newUser);
        _context.SaveChanges();

        return ReturnResult<Usuario>.Ok(newUser);
    }

    public ReturnResult<Usuario> ValidateLogin(LoginDTO dto)
    {
        var user = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email && u.SenhaHash == dto.Senha);

        if (user is null) return ReturnResult<Usuario>.Fail(StatusCode.NotFound, "Usuário não cadastrado");

        if (!BCrypt.Net.BCrypt.Verify(dto.Senha, user.SenhaHash))
            return false;

        return ReturnResult<Usuario>.Ok(user);
    }
}
