using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Domain.Models;

public class Veiculo
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public required string Nome { get; set; }

    [Required(ErrorMessage = "Marca é obrigatório")]
    [StringLength(100, ErrorMessage = "Marca deve ter no máximo 100 caracteres")]
    public required string Marca { get; set; }

    [Required(ErrorMessage = "Ano é obrigatório")]
    [Range(1900, 2100, ErrorMessage ="Ano deve estar entre 1900 e 2100")] 
    public required int Ano { get; set; }

    [Required(ErrorMessage = "Cor é obrigatório")]
    public required string Cor { get; set; }
    public required DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }   
}
