using AwesomePizza.Api.DTO.NotificationDTO;
using AwesomePizza.Api.Hubs;

using Microsoft.AspNetCore.SignalR;

namespace AwesomePizza.Api.Service.NotificationServices;

public class OrderNotifier : IOrderNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<OrderNotifier> _logger;

    public OrderNotifier(IHubContext<NotificationHub> hubContext, ILogger<OrderNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public Task NotifyOrderStatusChangedAsync(string code, string status, CancellationToken cancellationToken)
    {
        OrderStatusChangedDTO payload = new() { Code = code, Status = status };
        string group = NotificationEvents.OrderGroup(code);

        return SafeSendAsync(
            NotificationEvents.OrderStatusChanged,
            group,
            () => _hubContext.Clients.Group(group).SendAsync(NotificationEvents.OrderStatusChanged, payload, cancellationToken));
    }

    public Task NotifyQueueChangedAsync(CancellationToken cancellationToken)
    {
        return SafeSendAsync(
            NotificationEvents.QueueChanged,
            NotificationEvents.KitchenGroup,
            () => _hubContext.Clients.Group(NotificationEvents.KitchenGroup).SendAsync(NotificationEvents.QueueChanged, cancellationToken));
    }

    // The order is already saved, so a failed notification shouldn't break the request:
    // we log it and go on. The clirnt will get the right state from the API anyway
    private async Task SafeSendAsync(string eventName, string group, Func<Task> send)
    {
        try
        {
            await send();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not send {Event} to group {Group}", eventName, group);
        }
    }
}
