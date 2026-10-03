namespace AwesomePizza.Api.DTO.PizzaDTO;

public class GetPizzaDTO
{
    public int PizzaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
