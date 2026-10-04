using AwesomePizza.Api.Data;
using AwesomePizza.Api.Model;
using AwesomePizza.Api.Service;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomePizza.Tests.Integration;

[Collection(IntegrationCollection.Name)]
public abstract class IntegrationTestBase(ApiFactory factory) : IAsyncLifetime
{
    protected ApiFactory Factory { get; } = factory;

    // Every test starts with an empty queue; the order items go away with the orders (cascade delete)
    public async ValueTask InitializeAsync()
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        await context.Orders.ExecuteDeleteAsync(TestContext.Current.CancellationToken);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // Helper: an order with one line, created at the given time.
    protected static Order NewOrder(Pizza pizza, DateTimeOffset createdAt) =>
        new(OrderCodeGenerator.Generate(), null, [new OrderItem(pizza, 1)], createdAt);
}
