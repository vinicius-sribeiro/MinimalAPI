using MinimalAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Domain.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(255, ErrorMessage = "Nome deve ter no máximo 255 caracteres")]    
    public required string Nome { get; set; }
    
    [Required(ErrorMessage = "Nome é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Senha é obrigatória")]
    public required string SenhaHash { get; set; }

    public required Cargo Cargo { get; set; }

    public bool isAtivo { get; set; }

    public DateTime DataCriacao { get; set; }
}
