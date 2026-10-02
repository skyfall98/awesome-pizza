namespace AwesomePizza.Api.Model;

public class OrderItem
{
    
    private OrderItem() { }
    
    public OrderItem(Pizza pizza, int quantity)
    {
        Pizza = pizza;
        UnitPrice = pizza.Price;
        Quantity = quantity;
    }

    public int OrderItemId { get; private set; }
    public Order Order { get; private set; } = null!;
    public Pizza Pizza { get; private set; } = null!; 
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
}