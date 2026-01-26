using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Domain.DTOs;

public record LoginDTIO(
    [Required (ErrorMessage = "Email é obrigatório")]
    [EmailAddress (ErrorMessage = "Email inválido")]
    string Email,

    [Required (ErrorMessage = "Senha é obrigatória")]
    [MinLength(6 , ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    string Senha
);
