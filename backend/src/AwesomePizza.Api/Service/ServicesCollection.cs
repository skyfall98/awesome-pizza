using AwesomePizza.Api.Data;
using AwesomePizza.Api.Data.Seed;
using AwesomePizza.Api.DTO;
using AwesomePizza.Api.Service.KitchenServices;
using AwesomePizza.Api.Service.OrderItemServices;
using AwesomePizza.Api.Service.OrderServices;
using AwesomePizza.Api.Service.PizzaServices;

using Mapster;
using MapsterMapper;

using Microsoft.EntityFrameworkCore;

namespace AwesomePizza.Api.Service;

public static class ServicesCollection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AwesomePizza")));
        services.AddScoped<PizzaSeeder>();

        TypeAdapterConfig mapsterConfig = new();
        mapsterConfig.Scan(typeof(MapsterConfig).Assembly);
        services.AddSingleton(mapsterConfig);
        
        services.AddScoped<IPizzaService, PizzaService>();
        services.AddScoped<IOrderItemService, OrderItemService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IKitchenService, KitchenService>();

        return services;
    }
}
