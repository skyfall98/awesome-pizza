using AwesomePizza.Api.Data;
using AwesomePizza.Api.Enum;
using AwesomePizza.Api.Settings;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AwesomePizza.Api.Service.OrderCleanupServices;

public class OrderCleanupService: IOrderCleanupService
{
    private readonly DatabaseContext _context;
    private readonly OrderCleanupSettings _settings;
    private readonly ILogger<OrderCleanupService> _logger;

    public OrderCleanupService(DatabaseContext context, IOptions<OrderCleanupSettings> settings, ILogger<OrderCleanupService> logger)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<int> DeleteExpiredOrdersAsync(CancellationToken cancellationToken)
    {
        DateTimeOffset threshold = DateTimeOffset.UtcNow.AddDays(-_settings.RetentionDays);

        // Only ready orders are removed: the ones still in the queue are never touched.
        // The items go away with the order thanks to the cascade delete.
        int deleted = await _context.Orders
            .Where(o => o.Status == OrderStatusEnum.Ready && o.CreatedAt < threshold)
            .ExecuteDeleteAsync(cancellationToken);

        _logger.LogInformation("Order cleanup removed {Count} orders older than {Threshold:u}", deleted, threshold);

        return deleted;
    }
}
