using AwesomePizza.Api.Enum;
using AwesomePizza.Api.Model;

namespace AwesomePizza.Tests.Model;

public class OrderTests
{
    private static readonly Pizza Margherita = new() { PizzaId = 1, Name = "Margherita", Price = 6.50m };
    private static readonly Pizza Diavola = new() { PizzaId = 2, Name = "Diavola", Price = 8.00m };

    private static Order CreateOrder(params OrderItem[] items) =>
        new("ABC234", "Mario", items, DateTimeOffset.UtcNow);

    // A new order starts as Pending and keeps the values it was built with, including the given creation time.
    [Fact]
    public void Constructor_SetsPendingStatusAndGivenValues()
    {
        DateTimeOffset now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
        OrderItem item = new(Margherita, 2);

        Order order = new("ABC234", "Mario", [item], now);

        Assert.Equal(OrderStatusEnum.Pending, order.Status);
        Assert.Equal("ABC234", order.Code);
        Assert.Equal("Mario", order.CustomerName);
        Assert.Equal(now, order.CreatedAt);
        Assert.Single(order.Items);
    }

    // An order goes Pending -> InProgress -> Ready, using only the domain methods
    [Fact]
    public void StartThenComplete_MovesOrderToReady()
    {
        Order order = CreateOrder(new OrderItem(Margherita, 1));

        order.Start();
        Assert.Equal(OrderStatusEnum.InProgress, order.Status);

        order.Complete();
        Assert.Equal(OrderStatusEnum.Ready, order.Status);
    }

    // The kitchen must start an order before completing it: completing a Pending order is not allowed
    [Fact]
    public void Complete_WhenPending_Throws()
    {
        Order order = CreateOrder(new OrderItem(Margherita, 1));

        Assert.Throws<InvalidOperationException>(() => order.Complete());
    }

    // An order can be started only once: starting it again, or after it is Ready, is not allowed
    [Theory]
    [InlineData(OrderStatusEnum.InProgress)]
    [InlineData(OrderStatusEnum.Ready)]
    public void Start_WhenNotPending_Throws(OrderStatusEnum status)
    {
        Order order = CreateOrderWithStatus(status);

        Assert.Throws<InvalidOperationException>(() => order.Start());
    }

    // The total is the sum of the lines (unit price * quantity)
    [Fact]
    public void TotalPrice_SumsUnitPriceTimesQuantity()
    {
        Order order = CreateOrder(new OrderItem(Margherita, 2), new OrderItem(Diavola, 1));

        Assert.Equal(21.00m, order.TotalPrice);
    }

    // Helper: builds an order already in the given status
    private static Order CreateOrderWithStatus(OrderStatusEnum status)
    {
        Order order = CreateOrder(new OrderItem(Margherita, 1));

        if (status >= OrderStatusEnum.InProgress)
            order.Start();
        if (status >= OrderStatusEnum.Ready)
            order.Complete();

        return order;
    }
}
