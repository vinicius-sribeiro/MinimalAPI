using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Models;

namespace MinimalAPI.Domain.Interfaces;

public interface IUserService
{  
    Usuario? GetUserById(int id);    
}
