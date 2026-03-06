using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Models;
using MinimalAPI.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MinimalAPI_Test.Mocks
{
    internal class UsuarioServiceMock : IUsuarioService
    {

        private static List<Usuario> Usuarios = new List<Usuario>()
        {
            new Usuario { Id = 1, Nome = "Admin User", Email = "admin@teste.com", SenhaHash="hashed_123456", Cargo = Cargo.Admin, isAtivo = true, DataCriacao = DateTime.Now}
        };

        public Usuario? GetAdminById(int id)
        {
            var admin = Usuarios.Find(x => x.Id == id);
            if (admin != null && admin.Cargo == Cargo.Admin)
            {
                return admin;
            }

            return null;
        }

        public Usuario? GetUserById(int id)
        {
            return Usuarios.Find(x => x.Id == id);
        }

        public PagedResult<Usuario> ListAllUsers(AllUserListFilterDTO dto)
        {         
            var query = Usuarios.AsQueryable();

            if (!string.IsNullOrEmpty(dto.Email))
                query = query.Where(u => EF.Functions.Like(u.Email.ToLower(), $"%{dto.Email.ToLower()}%"));

            if (dto.Cargos.HasValue)            
                query = query.Where(x => x.Cargo == dto.Cargos.Value);

            if (dto.OrdernarCrescente)
                query = query.OrderBy(x => x.Nome);
            else
                query = query.OrderByDescending(x => x.Nome);

            var totalItens = query.Count();

            int pagina = dto.Pagina < 1 ? 1 : dto.Pagina;

            int pageSize = dto.PageSize switch
            {
                < 5 => 5,
                > 50 => 50,
                _ => dto.PageSize
            };

            var itens = query.Skip((pagina - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<Usuario>
            {
                PaginaAtual = pagina,
                PageSize = pageSize,
                TotalItens = totalItens,
                TotalPaginas = (int)Math.Ceiling(totalItens / (double)pageSize),
                Itens = itens
            };
        }


        public static List<Usuario> GetUsuarios()
        {
            return Usuarios;
        }        
    }
}
