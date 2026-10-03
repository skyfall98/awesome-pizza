using AwesomePizza.Api.Data;

using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace AwesomePizza.Api.Hubs;

// The hub only lets clients join a group: messages are sent from the server through IOrderNotifier.
// No user tracking here: SignalR keeps the groups and drops a connection when it closes.
public class NotificationHub : Hub
{
    private readonly DatabaseContext _context;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(DatabaseContext context, ILogger<NotificationHub> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogDebug("Client connected to the hub. ConnectionId: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    // The customer asks to follow an order. The code works as a credntial,
    // so we only let the client in if the order really exists
    public async Task SubscribeToOrder(string code)
    {
        string normalizedCode = code.ToUpperInvariant();

        bool exists = await _context.Orders
            .AnyAsync(o => o.Code == normalizedCode, Context.ConnectionAborted);

        if (!exists)
        {
            throw new HubException($"Order {code} not found.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, NotificationEvents.OrderGroup(normalizedCode));
        _logger.LogDebug("Connection {ConnectionId} subscribed to order {Code}", Context.ConnectionId, normalizedCode);
    }

    // Called by the kitchen page when it opens, to start receiving queue updates
    public Task JoinKitchen()
    {
        _logger.LogDebug("Connection {ConnectionId} joined the kitchen", Context.ConnectionId);
        return Groups.AddToGroupAsync(Context.ConnectionId, NotificationEvents.KitchenGroup);
    }
}
