using AwesomePizza.Api.Model;

using Microsoft.EntityFrameworkCore;

namespace AwesomePizza.Api.Data.Seed;

public class PizzaSeeder
{
    private readonly DatabaseContext _context;

    public PizzaSeeder(DatabaseContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        List<Pizza> desiredPizzas =
        [
            new Pizza { PizzaId = 1, Name = "Margherita", Price = 6.00m },
            new Pizza { PizzaId = 2, Name = "Marinara", Price = 5.00m },
            new Pizza { PizzaId = 3, Name = "Napoli", Price = 7.00m },
            new Pizza { PizzaId = 4, Name = "Diavola", Price = 7.50m },
            new Pizza { PizzaId = 5, Name = "Prosciutto e Funghi", Price = 8.00m },
            new Pizza { PizzaId = 6, Name = "Capricciosa", Price = 8.50m },
            new Pizza { PizzaId = 7, Name = "Quattro Stagioni", Price = 8.50m },
            new Pizza { PizzaId = 8, Name = "Quattro Formaggi", Price = 8.50m },
            new Pizza { PizzaId = 9, Name = "Bufala", Price = 9.00m },
            new Pizza { PizzaId = 10, Name = "Salsiccia", Price = 8.00m }
        ];

        HashSet<int> desiredIds = desiredPizzas.Select(p => p.PizzaId).ToHashSet();
        List<Pizza> existing = await _context.Pizzas.Where(p => desiredIds.Contains(p.PizzaId)).ToListAsync();
        Dictionary<int, Pizza> existingById = existing.ToDictionary(p => p.PizzaId);

        await _context.Database.OpenConnectionAsync();
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Pizzas ON;");

            foreach (Pizza target in desiredPizzas)
            {
                if (!existingById.TryGetValue(target.PizzaId, out Pizza? current))
                {
                    _context.Pizzas.Add(target);
                }
                else
                {
                    current.Name = target.Name;
                    current.Price = target.Price;
                }
            }

            await _context.SaveChangesAsync();
        }
        finally
        {
            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Pizzas OFF;");
            await _context.Database.CloseConnectionAsync();
        }
    }
}
