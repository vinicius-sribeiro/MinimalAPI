using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Models;

namespace MinimalAPI.Domain.Interfaces;

public interface IAdminService
{
    Usuario CreateAdmin(LoginDTO dto);
    bool ValidateAdminLogin(LoginDTO dto);
    Usuario? GetAdminById(int id);
    PagedResult<Usuario> ListAllUsers(AllUserListFilterDTO dto);
}
