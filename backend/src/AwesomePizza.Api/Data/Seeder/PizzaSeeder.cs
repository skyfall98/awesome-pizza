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
        List<Pizza> menu =
        [
            new(1, "Margherita", 6.00m),
            new(2, "Marinara", 5.00m),
            new(3, "Napoli", 7.00m),
            new(4, "Diavola", 7.50m),
            new(5, "Prosciutto e Funghi", 8.00m),
            new(6, "Capricciosa", 8.50m),
            new(7, "Quattro Stagioni", 8.50m),
            new(8, "Quattro Formaggi", 8.50m),
            new(9, "Bufalina", 9.00m),
            new(10, "Ortolana", 8.00m)
        ];

        HashSet<int> existingIds = (await _context.Pizzas.Select(p => p.PizzaId).ToListAsync()).ToHashSet();
        List<Pizza> missing = menu.Where(p => !existingIds.Contains(p.PizzaId)).ToList();
        if (missing.Count == 0) return;

        await _context.Database.OpenConnectionAsync();
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Pizzas ON;");
            _context.Pizzas.AddRange(missing);
            await _context.SaveChangesAsync();
        }
        finally
        {
            await _context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT dbo.Pizzas OFF;");
            await _context.Database.CloseConnectionAsync();
        }
    }
}