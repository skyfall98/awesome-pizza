using AwesomePizza.Api.Data;
using AwesomePizza.Api.DTO.OrderDTO;
using AwesomePizza.Api.Exceptions;
using AwesomePizza.Api.Model;
using AwesomePizza.Api.Service.OrderItemServices;

using Mapster;

using Microsoft.EntityFrameworkCore;

namespace AwesomePizza.Api.Service.OrderServices;

public class OrderService: IOrderService
{
    private readonly DatabaseContext _context;
    private readonly TypeAdapterConfig _mapsterConfig;
    private readonly IOrderItemService  _orderItemService;

    public OrderService(DatabaseContext context, TypeAdapterConfig mapsterConfig)
    {
        _context = context;
        _mapsterConfig = mapsterConfig;
    }


    public async Task<GetOrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDto, CancellationToken cancellationToken)
    {
        Order order = createOrderDto.Adapt<Order>();
        
        _context.Orders.Add(order);
        _context.SaveChanges();
        
        return cre
    }

    public async Task<GetOrderDTO> GetOrderByCodeAsync(string code, CancellationToken cancellationToken)
    {
        string normalizedCode = code.ToUpperInvariant();

        Order? order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Pizza)
            .FirstOrDefaultAsync(o => o.Code == normalizedCode, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order {code} not found.");
        }
        
        return order.Adapt<GetOrderDTO>(_mapsterConfig);
    }
}