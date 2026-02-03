using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Models;
using MinimalAPI.Enums;
using MinimalAPI.Infrastructure;

namespace MinimalAPI.Domain.Services;

public class UserService : IUserService
{
    public readonly MinimalApiContext _context;

    public UserService(MinimalApiContext context)
    {
        _context = context;
    }

    public Usuario CreateUser(LoginDTO dto)
    {
        var user = new Usuario
        {
            Email = dto.Email,
            Senha = dto.Senha,
            Cargo = Cargos.User
        };  

        _context.Usuarios.Add(user);
        _context.SaveChanges();

        return user;
    }

    public Usuario? GetUserById(int id)
    {
        var user = _context.Usuarios.Find(id);

        if (user is null || user.Cargo != Cargos.User) return null;

        return user;
    }    

    public bool ValidateUserLogin(LoginDTO dto)
    {
        var user = _context.Usuarios.FirstOrDefault(u => u.Email == dto.Email && u.Senha == dto.Senha); 

        if (user is null) return false;

        if (user.Cargo == Cargos.Admin)
        {
            return false;
        }

        return true;
    }
}
