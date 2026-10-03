using AwesomePizza.Api.DTO.OrderItemDTO;
using AwesomePizza.Api.Model;

namespace AwesomePizza.Api.Service.OrderItemServices;

public interface IOrderItemService
{
    Task<List<OrderItem>> BuildOrderItemsAsync(List<CreateOrderItemDTO> items, CancellationToken cancellationToken);
}
