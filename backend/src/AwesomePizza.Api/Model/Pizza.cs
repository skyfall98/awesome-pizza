namespace AwesomePizza.Api.Model;

public class Pizza
{
    public Pizza(int pizzaId, string name, decimal price)
    {
        PizzaId = pizzaId;
        Name = name;
        Price = price;
    }

    public int PizzaId { get; private set; }
    public string Name { get; private set; } = null!; 
    public decimal Price { get; private set; }
}