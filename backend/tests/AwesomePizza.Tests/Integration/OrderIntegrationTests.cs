using System.Net;
using System.Net.Http.Json;

using AwesomePizza.Api.Data;
using AwesomePizza.Api.DTO.OrderDTO;
using AwesomePizza.Api.DTO.OrderItemDTO;
using AwesomePizza.Api.Enum;
using AwesomePizza.Api.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AwesomePizza.Tests.Integration;

public class OrderIntegrationTests(ApiFactory factory) : IntegrationTestBase(factory)
{
    // A customer creates an order and then follows it with the code, typed in lowercase:
    // the code is case-insensitive and the order comes back Pending with the right total.
    [Fact]
    public async Task CreateOrder_ThenGetByCode_ReturnsTheOrder()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        using IServiceScope scope = Factory.Services.CreateScope();
        DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        Pizza pizza = await context.Pizzas.FirstAsync(cancellationToken);

        HttpClient client = Factory.CreateClient();
        CreateOrderDTO request = new()
        {
            CustomerName = "Mario",
            Items = [new CreateOrderItemDTO { PizzaId = pizza.PizzaId, Quantity = 2 }]
        };

        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/Order", request, cancellationToken);
        GetOrderDTO? created = await createResponse.Content.ReadFromJsonAsync<GetOrderDTO>(cancellationToken);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        HttpResponseMessage getResponse = await client.GetAsync($"/api/Order/{created!.Code.ToLowerInvariant()}", cancellationToken);
        GetOrderDTO? found = await getResponse.Content.ReadFromJsonAsync<GetOrderDTO>(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(created.Code, found!.Code);
        Assert.Equal(nameof(OrderStatusEnum.Pending), found.Status);
        Assert.Equal(pizza.Price * 2, found.TotalPrice);
    }

    // The same pizza on two lines is rejected: the API answers 400 with the reason in the ProblemDetails.
    [Fact]
    public async Task CreateOrder_WithDuplicatedPizza_ReturnsBadRequest()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        using IServiceScope scope = Factory.Services.CreateScope();
        DatabaseContext context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        Pizza pizza = await context.Pizzas.FirstAsync(cancellationToken);

        HttpClient client = Factory.CreateClient();
        CreateOrderDTO request = new()
        {
            Items =
            [
                new CreateOrderItemDTO { PizzaId = pizza.PizzaId, Quantity = 1 },
                new CreateOrderItemDTO { PizzaId = pizza.PizzaId, Quantity = 2 }
            ]
        };

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/Order", request, cancellationToken);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("The same pizza cannot appear on more than one line.", problem!.Detail);
    }
}
