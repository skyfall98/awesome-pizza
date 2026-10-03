using AwesomePizza.Api.DTO.OrderDTO;

namespace AwesomePizza.Api.Service.OrderServices;

public interface IOrderService
{
    Task<GetOrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDto, CancellationToken cancellationToken);
    Task<GetOrderDTO> GetOrderByCodeAsync(string code, CancellationToken cancellationToken);
}
