using AwesomePizza.Api.Data;
using AwesomePizza.Api.DTO.OrderDTO;
using AwesomePizza.Api.Enum;
using AwesomePizza.Api.Exceptions;
using AwesomePizza.Api.Model;

using Mapster;

using Microsoft.EntityFrameworkCore;

namespace AwesomePizza.Api.Service.KitchenServices;

public class KitchenService: IKitchenService
{
    private readonly DatabaseContext _context;
    private readonly TypeAdapterConfig _mapsterConfig;
    private readonly ILogger<KitchenService> _logger;

    public KitchenService(DatabaseContext context, TypeAdapterConfig mapsterConfig, ILogger<KitchenService> logger)
    {
        _context = context;
        _mapsterConfig = mapsterConfig;
        _logger = logger;
    }

    public async Task<List<GetOrderDTO>> GetQueueAsync(CancellationToken cancellationToken)
    {
        // the order now in progress is always the oldest one that is not ready, so it comes first
        List<Order> orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
                .ThenInclude(i => i.Pizza)
            .Where(o => o.Status != OrderStatusEnum.Ready)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync(cancellationToken);

        return orders.Adapt<List<GetOrderDTO>>(_mapsterConfig);
    }

    public async Task<GetOrderDTO> TakeNextOrderAsync(CancellationToken cancellationToken)
    {
        bool hasOrderInProgress = await _context.Orders
            .AnyAsync(o => o.Status == OrderStatusEnum.InProgress, cancellationToken);

        if (hasOrderInProgress)
        {
            throw new ConflictException("Another order is already in progress.");
        }

        Order? order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Pizza)
            .Where(o => o.Status == OrderStatusEnum.Pending)
            .OrderBy(o => o.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            throw new NotFoundException("There are no orders waiting.");
        }

        order.Start();

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The save failed: if another order is in progress now, another request got there first
            bool conflict = await _context.Orders
                .AnyAsync(o => o.Status == OrderStatusEnum.InProgress, cancellationToken);

            if (conflict)
            {
                throw new ConflictException("Another order is already in progress.");
            }

            // Not a conflict, throw the originl error with a 500
            throw;
        }

        _logger.LogInformation("Order {Code} taken by the kitchen", order.Code);

        return order.Adapt<GetOrderDTO>(_mapsterConfig);
    }

    public async Task<GetOrderDTO> CompleteOrderAsync(string code, CancellationToken cancellationToken)
    {
        string normalizedCode = code.ToUpperInvariant();

        Order? order = await _context.Orders
            .Include(o => o.Items)
                .ThenInclude(i => i.Pizza)
            .FirstOrDefaultAsync(o => o.Code == normalizedCode, cancellationToken);

        if (order is null)
        {
            throw new NotFoundException($"Order {code} not found.");
        }

        if (order.Status != OrderStatusEnum.InProgress)
        {
            throw new ConflictException($"Order {code} is not in progress.");
        }

        order.Complete();
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {Code} completed", order.Code);

        return order.Adapt<GetOrderDTO>(_mapsterConfig);
    }
}
