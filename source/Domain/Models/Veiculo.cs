using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Domain.Models;

public class Veiculo
{
    public int Id { get; set; }

    [StringLength(100)]
    public required string Nome { get; set; }

    [StringLength(100)]
    public required string Marca { get; set; }

    [Range(1900, 2100)] 
    public required int Ano { get; set; }
    public required string Cor { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }   
}
