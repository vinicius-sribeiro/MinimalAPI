using BCrypt.Net;
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

    public Usuario? GetUserById(int id)
    {
        var user = _context.Usuarios.Find(id);

        if (user is null || user.Cargo != Cargo.User) return null;

        return user;
    }       
}
