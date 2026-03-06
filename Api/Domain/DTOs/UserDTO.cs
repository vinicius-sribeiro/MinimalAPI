using MinimalAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Domain.DTOs;

/* ===================
*  AUTH
* ===================
*/

public record LoginDTO(
    [Required (ErrorMessage = "Email é obrigatório")]
    [EmailAddress (ErrorMessage = "Email inválido")]
    string Email,

    [Required (ErrorMessage = "Senha é obrigatória")]
    [MinLength(6 , ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    string Senha
);

public record RegisterDto(
    [Required (ErrorMessage = "Nome é obrigatório")]
    [MinLength(3, ErrorMessage = "Nome deve ter no mínimo 3 caracteres")]
    string Nome,

    [Required (ErrorMessage = "Email é obrigatório")]
    [EmailAddress (ErrorMessage = "Email inválido")]
    string Email,

    [Required (ErrorMessage = "Senha é obrigatória")]
    [MinLength(6 , ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    string Senha
);

public record TokenResponseDto(
    string Token,
    string TokenType,
    DateTime ExpiresAt,
    UsuarioDto Usuario
);
public record UsuarioDto(
    int Id,
    string Nome,
    string Email,
    string Role
);


/* ===================
*  FUNÇÕES/MÉTODOS DE USUÁRIO
* ===================
*/

public record OnlyUserListFilterDTO(
    int Pagina,
    int PageSize, // Quantidade de itens por página
    string? Email,
    bool OrdernarCrescente
);

public record AllUserListFilterDTO(
    int Pagina,
    int PageSize, // Quantidade de itens por página
    string? Email,
    Cargo? Cargos,
    bool OrdernarCrescente
);

