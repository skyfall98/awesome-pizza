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
    private readonly ILogger<OrderService> _logger;

    public OrderService(DatabaseContext context, TypeAdapterConfig mapsterConfig, IOrderItemService orderItemService, ILogger<OrderService> logger)
    {
        _context = context;
        _mapsterConfig = mapsterConfig;
        _orderItemService = orderItemService;
        _logger = logger;
    }

    public async Task<GetOrderDTO> CreateOrderAsync(CreateOrderDTO createOrderDto, CancellationToken cancellationToken)
    {
        List<OrderItem> orderItems = await _orderItemService.BuildOrderItemsAsync(createOrderDto.Items, cancellationToken);

        Order order = new Order(OrderCodeGenerator.Generate(), 
            createOrderDto.CustomerName, 
            orderItems, 
            DateTimeOffset.UtcNow);

        // The items are saved together with the order
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {Code} created", order.Code);

        return order.Adapt<GetOrderDTO>(_mapsterConfig);
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