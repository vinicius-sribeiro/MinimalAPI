using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Models;
using MinimalAPI.Enums;

namespace MinimalAPI.Domain.Interfaces;

public interface IAuthService
{
    ReturnResult<Usuario> RegisterUser(RegisterDto dto, Cargo cargo);
    ReturnResult<Usuario> ValidateLogin(LoginDTO dto);
}
