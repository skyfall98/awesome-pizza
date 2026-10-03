using AwesomePizza.Api.DTO.OrderItemDTO;
using AwesomePizza.Api.Exceptions;
using AwesomePizza.Api.Model;
using AwesomePizza.Api.Service.PizzaServices;

namespace AwesomePizza.Api.Service.OrderItemServices;

public class OrderItemService: IOrderItemService
{
    private readonly IPizzaService _pizzaService;

    public OrderItemService(IPizzaService pizzaService)
    {
        _pizzaService = pizzaService;
    }

    // Builds the items of a new order, rejecting duplicated or unknown pizzas
    // It does not save anything: the items are persisted together with the order
    
    public async Task<List<OrderItem>> BuildOrderItemsAsync(List<CreateOrderItemDTO> items,
        CancellationToken cancellationToken)
    {
        List<int> pizzaIds = items.Select(i => i.PizzaId).ToList();
        
        if (pizzaIds.Distinct().Count() != pizzaIds.Count)
        {
            throw new BadRequestException("The same pizza cannot appear on more than one line.");
        }

        List<Pizza> pizzas = await _pizzaService.GetPizzasByIdsAsync(pizzaIds,  cancellationToken); 
        
        //Check all the pizzas sent exist 
        if (pizzas.Count != pizzaIds.Count)
        {
            throw new BadRequestException("One or more pizzas do not exist.");
        }
        
        List<OrderItem> orderItems = new List<OrderItem>();
        
        foreach (CreateOrderItemDTO item in items)
        {
            Pizza pizza = pizzas.First(p => p.PizzaId == item.PizzaId);
            orderItems.Add(new OrderItem(pizza, item.Quantity));
        }

        return orderItems;
    }

}