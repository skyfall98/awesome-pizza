using AwesomePizza.Api.DTO.OrderItemDTO;
using AwesomePizza.Api.DTO.PizzaDTO;
using AwesomePizza.Api.Exceptions;
using AwesomePizza.Api.Model;
using AwesomePizza.Api.Service.OrderItemServices;
using AwesomePizza.Api.Service.PizzaServices;

namespace AwesomePizza.Tests.Unit;

public class OrderItemServiceTests
{
    private static readonly List<Pizza> Menu =
    [
        new() { PizzaId = 1, Name = "Margherita", Price = 6.50m },
        new() { PizzaId = 2, Name = "Diavola", Price = 8.00m }
    ];

    private readonly OrderItemService _service = new(new FakePizzaService(Menu));

    // The same pizza on two lines is rejected
    [Fact]
    public async Task BuildOrderItems_WithDuplicatedPizza_ThrowsBadRequest()
    {
        List<CreateOrderItemDTO> items =
        [
            new() { PizzaId = 1, Quantity = 1 },
            new() { PizzaId = 1, Quantity = 2 }
        ];

        await Assert.ThrowsAsync<BadRequestException>(
            () => _service.BuildOrderItemsAsync(items, TestContext.Current.CancellationToken));
    }

    // A pizza that is not on the menu is rejected
    [Fact]
    public async Task BuildOrderItems_WithUnknownPizza_ThrowsBadRequest()
    {
        List<CreateOrderItemDTO> items = [new() { PizzaId = 99, Quantity = 1 }];

        await Assert.ThrowsAsync<BadRequestException>(
            () => _service.BuildOrderItemsAsync(items, TestContext.Current.CancellationToken));
    }

    // Valid lines become order items with the menu price and the requested quantity
    [Fact]
    public async Task BuildOrderItems_WithValidItems_UsesMenuPrices()
    {
        List<CreateOrderItemDTO> items =
        [
            new() { PizzaId = 2, Quantity = 3 },
            new() { PizzaId = 1, Quantity = 1 }
        ];

        List<OrderItem> orderItems =
            await _service.BuildOrderItemsAsync(items, TestContext.Current.CancellationToken);

        Assert.Collection(orderItems,
            item =>
            {
                Assert.Equal(8.00m, item.UnitPrice);
                Assert.Equal(3, item.Quantity);
            },
            item =>
            {
                Assert.Equal(6.50m, item.UnitPrice);
                Assert.Equal(1, item.Quantity);
            });
    }

    // Hand-written fake: returns the pizzas of a fixed menu, no database involved
    private sealed class FakePizzaService(List<Pizza> menu) : IPizzaService
    {
        public Task<List<GetPizzaDTO>> GetAllPizzasAsync(CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<List<Pizza>> GetPizzasByIdsAsync(List<int> ids, CancellationToken cancellationToken) =>
            Task.FromResult(menu.Where(p => ids.Contains(p.PizzaId)).ToList());
    }
}
