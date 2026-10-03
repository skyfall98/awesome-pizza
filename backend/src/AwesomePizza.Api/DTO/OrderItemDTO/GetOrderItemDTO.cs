namespace AwesomePizza.Api.DTO.OrderItemDTO;

public class GetOrderItemDTO
{
    public string PizzaName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
