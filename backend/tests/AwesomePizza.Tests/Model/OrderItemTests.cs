using AwesomePizza.Api.Model;

namespace AwesomePizza.Tests.Model;

public class OrderItemTests
{
    // The line keeps the price the pizza had when the order was placed, even if the menu price changes later
    [Fact]
    public void Constructor_SnapshotsPizzaPrice()
    {
        Pizza pizza = new() { PizzaId = 1, Name = "Margherita", Price = 6.50m };

        OrderItem item = new(pizza, 2);
        pizza.Price = 9.00m;

        Assert.Equal(6.50m, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
    }
}
