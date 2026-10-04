using System.Net;
using System.Net.Http.Json;

using AwesomePizza.Api.Data;
using AwesomePizza.Api.DTO.OrderDTO;
using AwesomePizza.Api.Enum;
using AwesomePizza.Api.Model;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomePizza.Tests.Integration;

public class KitchenIntegrationTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    private const string TakeNextUrl = "/api/Kitchen/queue/next";

    // The kitchen always gets the oldest pending order, and cannot take another one while it is working on one.
    [Fact]
    public async Task TakeNext_ReturnsOldestPendingOrder_ThenConflictWhileOneIsInProgress()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        using IServiceScope scope = Factory.Services.CreateScope();
        DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        Pizza pizza = await context.Pizzas.FirstAsync(cancellationToken);

        // Inserted out of order on purpose: the queue must follow CreatedAt, not the insertion order
        Order middle = NewOrder(pizza, now.AddHours(-2));
        Order newest = NewOrder(pizza, now.AddHours(-1));
        Order oldest = NewOrder(pizza, now.AddHours(-3));
        context.Orders.AddRange(middle, newest, oldest);
        await context.SaveChangesAsync(cancellationToken);

        HttpClient client = Factory.CreateClient();

        HttpResponseMessage first = await client.PostAsync(TakeNextUrl, null, cancellationToken);
        GetOrderDTO? taken = await first.Content.ReadFromJsonAsync<GetOrderDTO>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(oldest.Code, taken!.Code);
        Assert.Equal(nameof(OrderStatusEnum.InProgress), taken.Status);

        HttpResponseMessage second = await client.PostAsync(TakeNextUrl, null, cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    // The kitchen takes an order and completes it: the order becomes Ready, and completing it again is a conflict.
    [Fact]
    public async Task CompleteOrder_AfterTakeNext_MovesOrderToReady()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        using IServiceScope scope = Factory.Services.CreateScope();
        DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        Pizza pizza = await context.Pizzas.FirstAsync(cancellationToken);

        Order order = NewOrder(pizza, DateTimeOffset.UtcNow);
        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        HttpClient client = Factory.CreateClient();
        await client.PostAsync(TakeNextUrl, null, cancellationToken);

        string completeUrl = $"/api/Kitchen/orders/{order.Code}/complete";
        HttpResponseMessage first = await client.PostAsync(completeUrl, null, cancellationToken);
        GetOrderDTO? completed = await first.Content.ReadFromJsonAsync<GetOrderDTO>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(nameof(OrderStatusEnum.Ready), completed!.Status);

        HttpResponseMessage second = await client.PostAsync(completeUrl, null, cancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    // Many requests arrive at the same time: whatever the outcome, only one order can end up InProgress.
    // A repeated 200 for the same order is accepted: it is harmless with a single pizza maker and the unique index
    // still guarantees the invariant, so no optimistic concurrency token is needed.
    [Fact]
    public async Task TakeNext_WithConcurrentRequests_StartsOnlyOneOrder()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        using IServiceScope scope = Factory.Services.CreateScope();
        DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        Pizza pizza = await context.Pizzas.FirstAsync(cancellationToken);

        context.Orders.AddRange(NewOrder(pizza, now.AddHours(-2)), NewOrder(pizza, now.AddHours(-1)));
        await context.SaveChangesAsync(cancellationToken);

        HttpClient client = Factory.CreateClient();

        HttpResponseMessage[] responses = await Task.WhenAll(
            Enumerable.Range(0, 5).Select(_ => client.PostAsync(TakeNextUrl, null, cancellationToken)));

        Assert.All(responses, r => Assert.True(r.StatusCode is HttpStatusCode.OK or HttpStatusCode.Conflict));
        Assert.Contains(responses, r => r.StatusCode == HttpStatusCode.OK);
        Assert.Equal(1, await context.Orders.CountAsync(o => o.Status == OrderStatusEnum.InProgress, cancellationToken));
    }
}
