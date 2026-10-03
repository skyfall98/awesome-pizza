using AwesomePizza.Api.Service.OrderCleanupServices;
using AwesomePizza.Api.Settings;

using Microsoft.Extensions.Options;

namespace AwesomePizza.Api.BackgroundJobs;

public class OrderCleanupWorker: BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OrderCleanupSettings _settings;
    private readonly ILogger<OrderCleanupWorker> _logger;

    public OrderCleanupWorker(IServiceScopeFactory scopeFactory, IOptions<OrderCleanupSettings> settings, ILogger<OrderCleanupWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // First run at startup, then once per interval
        using PeriodicTimer timer = new(TimeSpan.FromHours(_settings.IntervalHours));

        do
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                IOrderCleanupService cleanupService = scope.ServiceProvider.GetRequiredService<IOrderCleanupService>();
                await cleanupService.DeleteExpiredOrdersAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // An unhandled exception would stop the whole host: log it and retry at the next run
                _logger.LogError(ex, "Order cleanup failed");
            }
        }
        while (await WaitForNextRunAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitForNextRunAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
