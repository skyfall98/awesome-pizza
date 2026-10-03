namespace AwesomePizza.Api.Hubs;

// Event names and group names 
public static class NotificationEvents
{
    // Sent to the customer watching one order
    public const string OrderStatusChanged = "OrderStatusChanged";

    // Sent to the kitchen: "something changed in the queue, fetch it again"
    public const string QueueChanged = "QueueChanged";

    public const string KitchenGroup = "kitchen";

    public static string OrderGroup(string code) => $"order-{code.ToUpperInvariant()}";
}
