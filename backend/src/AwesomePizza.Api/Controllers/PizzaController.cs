using AwesomePizza.Api.DTO.PizzaDTO;
using AwesomePizza.Api.Service.PizzaServices;

using Microsoft.AspNetCore.Mvc;

namespace AwesomePizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PizzaController: ControllerBase
{
    private readonly IPizzaService pizzaService;

    public PizzaController(IPizzaService pizzaService)
    {
        this.pizzaService = pizzaService;
    }

    [HttpGet]
    public async Task<ActionResult<List<GetPizzaDTO>>> GetAllPizzas(CancellationToken cancellationToken)
    {
        List<GetPizzaDTO> pizzas = await pizzaService.GetAllPizzasAsync(cancellationToken);
        return Ok(pizzas);
    }
}
