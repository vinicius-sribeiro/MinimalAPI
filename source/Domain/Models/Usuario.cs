using MinimalAPI.Enums;
using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Domain.Models;

public class Usuario
{
    public int Id { get; set; }

    [StringLength(255)]
    public required string Email { get; set; }

    [StringLength(50)]
    public required string Senha { get; set; }

    public required Cargos Cargo { get; set; }
}
