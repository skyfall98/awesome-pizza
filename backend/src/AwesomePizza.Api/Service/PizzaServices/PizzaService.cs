using AwesomePizza.Api.Data;
using AwesomePizza.Api.DTO.PizzaDTO;
using AwesomePizza.Api.Model;

using Mapster;

using Microsoft.EntityFrameworkCore;

namespace AwesomePizza.Api.Service.PizzaServices;

public class PizzaService: IPizzaService
{
    
    private readonly TypeAdapterConfig _mapsterConfig;
    private readonly DatabaseContext  _context;

    public PizzaService (TypeAdapterConfig config, DatabaseContext context)
    {
        
        _mapsterConfig = config;
        _context = context;
    }

    public async Task<List<GetPizzaDTO>> GetAllPizzasAsync(CancellationToken cancellationToken)
    {
        List<Pizza> pizzas = await _context.Pizzas
            .AsNoTracking()
            .OrderBy(p=>p.Price)
            .ToListAsync(cancellationToken);
        
        return pizzas.Adapt<List<GetPizzaDTO>>(_mapsterConfig); 
    }

    public async Task<List<Pizza>> GetPizzasByIdsAsync(List<int> ids, CancellationToken cancellationToken)
    {
        return await _context.Pizzas
            .Where(p => ids.Contains(p.PizzaId))
            .ToListAsync(cancellationToken);
    }
}