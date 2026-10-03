using AwesomePizza.Api.DTO.OrderItemDTO;
using AwesomePizza.Api.Model;

using Mapster;

namespace AwesomePizza.Api.DTO;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<OrderItem, GetOrderItemDTO>()
            .Map(dest => dest.PizzaName, src => src.Pizza.Name);
    }
}
