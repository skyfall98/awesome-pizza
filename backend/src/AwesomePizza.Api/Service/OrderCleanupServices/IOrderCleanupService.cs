namespace AwesomePizza.Api.Service.OrderCleanupServices;

public interface IOrderCleanupService
{
    /// <summary>Deletes the ready orders older than the retention period and returns how many were removed.</summary>
    Task<int> DeleteExpiredOrdersAsync(CancellationToken cancellationToken);
}
