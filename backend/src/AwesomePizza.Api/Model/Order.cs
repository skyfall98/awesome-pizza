using AwesomePizza.Api.Enum;

namespace AwesomePizza.Api.Model;

public class Order
{
    private readonly List<OrderItem> _items = [];

    private Order() { }

    public Order(string code, string? customerName, IEnumerable<OrderItem> items, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7();
        Code = code;
        CustomerName = customerName;
        CreatedAt = now;
        Status = OrderStatusEnum.Pending;
        _items.AddRange(items);
    }

    public Guid Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string? CustomerName { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public OrderStatusEnum Status { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;
    public decimal TotalPrice => _items.Sum(i => i.UnitPrice * i.Quantity);

    public void Start()
    {
        if (Status != OrderStatusEnum.Pending)
            throw new InvalidOperationException($"Cannot start an order that is {Status}.");

        Status = OrderStatusEnum.InProgress;
    }

    public void Complete()
    {
        if (Status != OrderStatusEnum.InProgress)
            throw new InvalidOperationException($"Cannot complete an order that is {Status}.");

        Status = OrderStatusEnum.Ready;
    }
}