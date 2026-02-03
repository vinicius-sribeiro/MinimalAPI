using Microsoft.EntityFrameworkCore;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.Models;
using MinimalAPI.Infrastructure;
using System;
using System.Security.Authentication;

namespace MinimalAPI.Domain.Services;

public class VeiculoService : IVeiculoService
{
    public readonly MinimalApiContext _context;

    public VeiculoService(MinimalApiContext context)
    {
        _context = context;
    }

    public Veiculo AddVeiculo(AddVeiculoDTO dto)
    {
        var veiculo = new Veiculo
        {
            Nome = dto.Nome,
            Marca = dto.Marca,
            Ano = dto.Ano,
            Cor = dto.Cor,
            CreatedAt = DateTime.Now,
        };

        _context.Veiculos.Add(veiculo);
        _context.SaveChanges();

        return veiculo;
    }

    public void UpdateVeiculo(int id, UpdateVeiculoDTO dto)
    {
        bool houveAlteracao = false;

        var veiculo = _context.Veiculos.Find(id);

        if (veiculo == null)
            throw new KeyNotFoundException();

        if (string.IsNullOrWhiteSpace(dto.Nome) &&
            string.IsNullOrWhiteSpace(dto.Marca) &&
            dto.Ano == null &&
            string.IsNullOrWhiteSpace(dto.Cor))
        {
            throw new ArgumentException("Nenhum campo para atualizar foi fornecido.");
        }

        if (!string.IsNullOrWhiteSpace(dto.Nome))
        {
            veiculo.Nome = dto.Nome;
            houveAlteracao = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Marca))
        {
            veiculo.Marca = dto.Marca;
            houveAlteracao = true;
        }

        if (dto.Ano is not null)
        {
            veiculo.Ano = dto.Ano.Value;
            houveAlteracao = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Cor))
        {
            veiculo.Cor = dto.Cor;
            houveAlteracao = true;
        }

        if (houveAlteracao) veiculo.UpdatedAt = DateTime.Now;

        _context.Veiculos.Update(veiculo);
        _context.SaveChanges();
    }
    public void RemoveById(int id)
    {
        var veiculo = _context.Veiculos.Find(id);

        if (veiculo == null)
            throw new KeyNotFoundException();

        _context.Veiculos.Remove(veiculo);
        _context.SaveChanges();
    }

    public Veiculo? GetSpecificById(int id)
    {
        var veiculo = _context.Veiculos.Find(id);

        if (veiculo == null)
            return null;

        return veiculo;
    }

    public PagedResult<Veiculo> ListAll(VeiculoListFilterDTO filters)
    {
        IQueryable<Veiculo> query = _context.Veiculos;

        // 1 - Aplicar filtros
        if (!string.IsNullOrEmpty(filters.Nome))
            query = query.Where(v => EF.Functions.Like(v.Nome.ToLower(), $"%{filters.Nome.ToLower()}%"));
        if (!string.IsNullOrEmpty(filters.Marca))
            query = query.Where(v => EF.Functions.Like(v.Marca.ToLower(), $"%{filters.Marca.ToLower()}%"));
        if (!string.IsNullOrEmpty(filters.Cor))
            query = query.Where(v => EF.Functions.Like(v.Cor.ToLower(), $"%{filters.Cor.ToLower()}%"));
        if (filters.Ano.HasValue)
            query = query.Where(v => v.Ano == filters.Ano.Value);

        // 2 - Normalização de paginação
        int pagina = filters.Pagina < 1 ? 1 : filters.Pagina;

        int pageSize = filters.PageSize switch
        {
            < 5 => 5,
            > 50 => 50,
            _ => filters.PageSize
        };

        // 3 - Total de itens antes da paginação
        int totalItens = query.Count();

        // 4 - Ordenação
        query = filters.OrdernarCrescente
            ? query.OrderByDescending(v => v.Nome)
            : query.OrderBy(v => v.Nome);


        // 5 - paginação
        int skip = (pagina - 1) * pageSize;

        var itens = query
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        // 6 - Resultado da paginação

        return new PagedResult<Veiculo>
        {
            PaginaAtual = pagina,
            PageSize = pageSize,
            TotalItens = totalItens,
            TotalPaginas = (int)Math.Ceiling((double)totalItens / pageSize),
            Itens = itens
        };
    }

}
