using AwesomePizza.Api.DTO.OrderItemDTO;

namespace AwesomePizza.Api.DTO.OrderDTO;

public class GetOrderDTO
{
    public string Code { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public decimal TotalPrice { get; set; }
    public List<GetOrderItemDTO> Items { get; set; } = [];
}
