using AwesomePizza.Api.DTO.PizzaDTO;
using AwesomePizza.Api.Model;

namespace AwesomePizza.Api.Service.PizzaServices;

public interface IPizzaService
{
    Task<List<GetPizzaDTO>> GetAllPizzasAsync(CancellationToken cancellationToken);
    Task<List<Pizza>> GetPizzasByIdsAsync(List<int> ids, CancellationToken cancellationToken);
}