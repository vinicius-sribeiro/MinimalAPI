using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Models;
using MinimalAPI.Enums;
using MinimalAPI.Extensions;
using MinimalAPI.Infrastructure;
using System.Linq.Expressions;

namespace MinimalAPI.Domain.Services;

public class AuthService : IAuthService
{
    private readonly MinimalApiContext _context;
    private readonly ITokenService _tokenService;   
    private readonly IUserContext _userContext;

    public AuthService(MinimalApiContext context, ITokenService tokenService, IUserContext userContext)
    {
        _context = context;
        _tokenService = tokenService;
        _userContext = userContext;
    }

    public ReturnAuthResult<Usuario> RegisterUser(RegisterDto dto, Cargo cargo)
    {
        // Validar se email já existe
        var email = dto.Email.ToLowerInvariant();
        if (_context.Usuarios.Any(u => u.Email.ToLower() == email))
        {
            return ReturnAuthResult<Usuario>.Fail(AuthErrorType.EmailAlreadyExists, "Email já cadastrado");
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

        _context.Usuarios.Add(newUser);
        _context.SaveChanges();

        // Gerar Token
        var token = _tokenService.GenerateToken(newUser);
        var expiresAt = _tokenService.ObterDataExpiracao();

        var response = new TokenResponseDTO(
            Token: token,
            TokenType: "Bearer",
            ExpiresAt: expiresAt,
            Usuario: new TokenUsuarioDTO(newUser.Id, newUser.Nome, newUser.Email, newUser.Cargo)
        );     

        return ReturnAuthResult<Usuario>.Ok(newUser, response);
    }

    public ReturnAuthResult<Usuario> ValidateLogin(LoginDTO dto)
    {
        var user = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email.ToLower());

        if (user is null) 
            return ReturnAuthResult<Usuario>.Fail(AuthErrorType.UserNotFound, "Usuário não cadastrado.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Senha, user.SenhaHash))
            return ReturnAuthResult<Usuario>.Fail(AuthErrorType.InvalidCredentials);

        if (!user.isAtivo)
            return ReturnAuthResult<Usuario>.Fail(AuthErrorType.InactiveAccount, "Sua conta está inativa. Entre em contato com o suporte.");

        // Gerar Token
        var token = _tokenService.GenerateToken(user);
        var expiresAt = _tokenService.ObterDataExpiracao();

        var tokenResponse = new TokenResponseDTO(
            Token: token,
            TokenType: "Bearer",
            ExpiresAt: expiresAt,
            Usuario: new TokenUsuarioDTO(user.Id, user.Nome, user.Email, user.Cargo)
        );

        return ReturnAuthResult<Usuario>.Ok(user, tokenResponse);
    }

    public ReturnAuthResult<TokenUsuarioDTO> GetMe()
    {
        if (!_userContext.TryGetUserId(out int userId))
            return ReturnAuthResult<TokenUsuarioDTO>.Fail(AuthErrorType.Unauthorized, "Usuário não autorizado");

        var user = _context.Usuarios.AsNoTracking().FirstOrDefault(u => u.Id == userId);
        if (user == null)
            return ReturnAuthResult<TokenUsuarioDTO>.Fail(AuthErrorType.UserNotFound, "Usuário não encontrado.");

        var dto = new TokenUsuarioDTO(user.Id, user.Nome, user.Email, user.Cargo);

        return ReturnAuthResult<TokenUsuarioDTO>.Ok(dto);
    }   
}
