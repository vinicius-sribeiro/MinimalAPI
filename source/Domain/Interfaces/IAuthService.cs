using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Models;
using MinimalAPI.Enums;

namespace MinimalAPI.Domain.Interfaces;

public interface IAuthService
{
    ReturnAuthResult<Usuario> RegisterUser(RegisterDto dto, Cargo cargo);
    ReturnAuthResult<Usuario> ValidateLogin(LoginDTO dto);
    ReturnAuthResult<TokenUsuarioDTO> GetMe();
}
