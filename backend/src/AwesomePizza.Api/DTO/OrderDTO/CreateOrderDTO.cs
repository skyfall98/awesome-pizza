using System.ComponentModel.DataAnnotations;

using AwesomePizza.Api.DTO.OrderItemDTO;

namespace AwesomePizza.Api.DTO.OrderDTO;

public class CreateOrderDTO
{
    [MaxLength(100)]
    public string? CustomerName { get; set; }

    [MinLength(1)]
    public List<CreateOrderItemDTO> Items { get; set; } = [];
}
