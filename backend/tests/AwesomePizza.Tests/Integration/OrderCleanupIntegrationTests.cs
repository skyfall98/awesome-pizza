using AwesomePizza.Api.Data;
using AwesomePizza.Api.Model;
using AwesomePizza.Api.Service.OrderCleanupServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomePizza.Tests.Integration;

public class OrderCleanupIntegrationTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    // Only the Ready orders older than the retention (7 days) are deleted, together with their items
    [Fact]
    public async Task DeleteExpiredOrders_RemovesOnlyOldReadyOrders()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        DateTimeOffset now = DateTimeOffset.UtcNow;

        using IServiceScope scope = Factory.Services.CreateScope();
        DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        Pizza pizza = await context.Pizzas.FirstAsync(cancellationToken);

        Order oldReady = NewOrder(pizza, now.AddDays(-10));
        oldReady.Start();
        oldReady.Complete();

        Order recentReady = NewOrder(pizza, now.AddDays(-1));
        recentReady.Start();
        recentReady.Complete();

        Order oldPending = NewOrder(pizza, now.AddDays(-10));

        Order oldInProgress = NewOrder(pizza, now.AddDays(-10));
        oldInProgress.Start();

        context.Orders.AddRange(oldReady, recentReady, oldPending, oldInProgress);
        await context.SaveChangesAsync(cancellationToken);

        IOrderCleanupService cleanupService = scope.ServiceProvider.GetRequiredService<IOrderCleanupService>();

        int deleted = await cleanupService.DeleteExpiredOrdersAsync(cancellationToken);

        List<string> remainingCodes = await context.Orders.AsNoTracking().Select(o => o.Code).ToListAsync(cancellationToken);

        Assert.Equal(1, deleted);
        Assert.DoesNotContain(oldReady.Code, remainingCodes);
        Assert.Equal(3, remainingCodes.Count);
        Assert.Equal(3, await context.OrderItems.CountAsync(cancellationToken));
    }
}
