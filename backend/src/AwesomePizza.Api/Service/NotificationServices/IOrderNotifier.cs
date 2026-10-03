namespace AwesomePizza.Api.Service.NotificationServices;

public interface IOrderNotifier
{
    Task NotifyOrderStatusChangedAsync(string code, string status, CancellationToken cancellationToken);
    Task NotifyQueueChangedAsync(CancellationToken cancellationToken);
}
