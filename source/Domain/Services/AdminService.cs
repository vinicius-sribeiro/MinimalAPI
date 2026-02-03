using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Models;
using MinimalAPI.Enums;
using MinimalAPI.Infrastructure;

namespace MinimalAPI.Domain.Services;

public class AdminService : IAdminService
{
    public readonly MinimalApiContext _context;

    public AdminService(MinimalApiContext context)
    {
        _context = context;
    }

    public Usuario CreateAdmin(LoginDTO dto)
    {
        var admin = new Usuario
        {
            Email = dto.Email,
            Senha = dto.Senha,
            Cargo = Cargos.Admin
        };

        _context.Usuarios.Add(admin);
        _context.SaveChanges();

        return admin;
    }
    public bool ValidateAdminLogin(LoginDTO dto)
    {
        var admin = _context.Usuarios.FirstOrDefault(a => a.Email == dto.Email && a.Senha == dto.Senha);

        if (admin is null) return false;

        if (admin.Cargo == Cargos.User)
        {
            return false;
        }

        return true;
    }

    public Usuario? GetAdminById(int id)
    {
        var admin = _context.Usuarios.Find(id);

        if (admin is null || admin.Cargo != Cargos.Admin)
        {
            return null;
        }

        return admin;
    }

    public PagedResult<Usuario> ListAllUsers(AllUserListFilterDTO dto)
    {
        IQueryable<Usuario> query = _context.Usuarios;

        if (!string.IsNullOrEmpty(dto.Email))
            query = query.Where(u => EF.Functions.Like(u.Email.ToLower(), $"%{dto.Email.ToLower()}%");

        if (dto.Cargos.HasValue)
            query = query.Where(u => u.Cargo == dto.Cargos.Value);

        query = dto.OrdernarCrescente
            ? query.OrderBy(u => u.Email)
            : query.OrderByDescending(u => u.Email);

        var totalItens = query.Count();

        int pagina = dto.Pagina < 1 ? 1 : dto.Pagina;

        int pageSize = dto.PageSize switch
        {
            < 5 => 5,
            > 50 => 50,
            _ => dto.PageSize
        };

        var itens = query
            .Skip((pagina - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<Usuario>
        {
            PaginaAtual = pagina,
            PageSize = pageSize,
            TotalItens = totalItens,
            TotalPaginas = (int)Math.Ceiling(totalItens / (double)pageSize),
            Itens = itens
        };
    }


}
