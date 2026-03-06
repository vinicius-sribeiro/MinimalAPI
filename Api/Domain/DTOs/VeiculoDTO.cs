using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using MinimalAPI.Domain.DataAnnotations;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MinimalAPI.Domain.DTOs;

// CRIAR - Só dados necessários para criação
public record AddVeiculoDTO (
    [Required (ErrorMessage = "Nome é obrigatório")]
    [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    string Nome,

    [Required (ErrorMessage = "Marca é obrigatória")]
    [MaxLength(100, ErrorMessage = "Marca deve ter no máximo 100 caracteres")]
    string Marca,

    [Required (ErrorMessage = "Ano é obrigatório")]
    [AnoValidate(ErrorMessage = "Ano inválido")]
    int Ano,

    [Required (ErrorMessage = "Cor é obrigatória")]
    string Cor
);

// ATUALIZAR - Só campos que usuário pode alterar
public record UpdateVeiculoDTO (
    string? Nome,
    string? Marca,
    int? Ano,
    string? Cor
);

public record VeiculoListFilterDTO (
    int Pagina,  
    int PageSize, // Quantidade de itens por página
    string? Nome,
    string? Marca,
    string? Cor,
    int? Ano,
    bool OrdernarCrescente
);