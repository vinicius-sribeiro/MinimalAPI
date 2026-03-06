using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Models;
using MinimalAPI.Enums;

namespace MinimalAPI_Test.Mocks
{
    internal class AuthServiceMock : IAuthService
    {        
        private readonly ITokenService _tokenService;
        public AuthServiceMock(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public ReturnAuthResult<TokenUsuarioDTO> GetMe()
        {
            throw new NotImplementedException();
        }

        public ReturnAuthResult<Usuario> RegisterUser(RegisterDto dto, Cargo cargo)
        {
            var email = dto.Email.ToLowerInvariant();
            if (UsuarioServiceMock.GetUsuarios().Any(u => u.Email == email))
            {
                return ReturnAuthResult<Usuario>.Fail(AuthErrorType.EmailAlreadyExists, "Email já registrado.");
            }

            var newUser = new Usuario
            {
                Id = UsuarioServiceMock.GetUsuarios().Count + 1,
                Nome = dto.Nome,
                Email = email,
                SenhaHash = $"hashed_{dto.Senha}",
                Cargo = cargo,
                isAtivo = true,
                DataCriacao = DateTime.UtcNow
            };

            UsuarioServiceMock.GetUsuarios().Add(newUser);

            return ReturnAuthResult<Usuario>.Ok(newUser);
        }

        public ReturnAuthResult<Usuario> ValidateLogin(LoginDTO dto)
        {
            var user = UsuarioServiceMock.GetUsuarios().FirstOrDefault(u => u.Email == dto.Email.ToLower());

            if (user is null)
                return ReturnAuthResult<Usuario>.Fail(AuthErrorType.UserNotFound, "Usuário não cadastrado.");

            var senha = $"hashed_{dto.Senha}";
            if (senha != user.SenhaHash)
                return ReturnAuthResult<Usuario>.Fail(AuthErrorType.UserNotFound);

            if (!user.isAtivo)
                return ReturnAuthResult<Usuario>.Fail(AuthErrorType.InactiveAccount, "Sua conta está inativa. Entre em contato com o suporte.");

            return ReturnAuthResult<Usuario>.Ok(user,
                new TokenResponseDTO(
                    Token: "",
                    TokenType: "",
                    ExpiresAt: DateTime.MaxValue,
                    Usuario: new TokenUsuarioDTO(user.Id, user.Nome, user.Email, user.Cargo)
                ));
        }
    }
}