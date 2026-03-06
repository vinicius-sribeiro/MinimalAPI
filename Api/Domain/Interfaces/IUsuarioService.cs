using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Models;

namespace MinimalAPI.Domain.Interfaces;

public interface IUsuarioService
{  
    Usuario? GetUserById(int id);
    public Usuario? GetAdminById(int id);
    public PagedResult<Usuario> ListAllUsers(AllUserListFilterDTO dto);
}
