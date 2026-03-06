using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Models;

namespace MinimalAPI.Domain.Interfaces;

public interface IVeiculoService
{
    Veiculo AddVeiculo(AddVeiculoDTO dto);

    void UpdateVeiculo(int id, UpdateVeiculoDTO dto);

    void RemoveById(int id);

    Veiculo? GetSpecificById(int id);

    PagedResult<Veiculo> ListAll(VeiculoListFilterDTO filters);

}
