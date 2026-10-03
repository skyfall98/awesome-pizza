using AwesomePizza.Api.DTO.OrderDTO;

namespace AwesomePizza.Api.Service.KitchenServices;

public interface IKitchenService
{
    Task<List<GetOrderDTO>> GetQueueAsync(CancellationToken cancellationToken);
    Task<GetOrderDTO> TakeNextOrderAsync(CancellationToken cancellationToken);
    Task<GetOrderDTO> CompleteOrderAsync(string code, CancellationToken cancellationToken);
}
