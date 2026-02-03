using MinimalAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Domain.DTOs;

public record LoginDTO(
    [Required (ErrorMessage = "Email é obrigatório")]
    [EmailAddress (ErrorMessage = "Email inválido")]
    string Email,

    [Required (ErrorMessage = "Senha é obrigatória")]
    [MinLength(6 , ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    string Senha
);

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
    Cargos? Cargos,
    bool OrdernarCrescente
);

