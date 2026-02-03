using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Models;

namespace MinimalAPI.Domain.Interfaces;

public interface IUserService
{
    Usuario CreateUser(LoginDTO dto);
    bool ValidateUserLogin(LoginDTO dto);
    Usuario? GetUserById(int id);    
}
