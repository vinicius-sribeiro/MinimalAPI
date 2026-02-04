using MinimalAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Domain.Models;

public class Usuario
{
    public int Id { get; set; }

    [StringLength(255)]
    public required string Nome { get; set; }

    [StringLength(255)]
    public required string Email { get; set; }

    [StringLength(50)]
    public required string SenhaHash { get; set; }

    public required Cargo Cargo { get; set; }

    public bool isAtivo { get; set; }

    public DateTime DataCriacao { get; set; }
}
