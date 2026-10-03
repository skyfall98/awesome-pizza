using System.ComponentModel.DataAnnotations;

namespace AwesomePizza.Api.DTO.OrderItemDTO;

public class CreateOrderItemDTO
{
    public int PizzaId { get; set; }

    [Range(1, 100)]
    public int Quantity { get; set; }
}
