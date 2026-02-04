using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Models;

namespace MinimalAPI.Domain.Interfaces;

public interface IAdminService
{  
    Usuario? GetAdminById(int id);
    PagedResult<Usuario> ListAllUsers(AllUserListFilterDTO dto);
}
