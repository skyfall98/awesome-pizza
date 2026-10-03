using AwesomePizza.Api.Model;

using Microsoft.EntityFrameworkCore;

namespace AwesomePizza.Api.Data;

public class DatabaseContext: DbContext
{
    public DatabaseContext(): base() { }
    public DatabaseContext(DbContextOptions<DatabaseContext> options): base(options) { }
    
    public virtual DbSet<Order> Orders  { get; set; }
    public virtual DbSet<Pizza> Pizzas { get; set; }
    public virtual DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Pizza>().Property(p => p.Price).HasPrecision(10, 2);
        modelBuilder.Entity<OrderItem>().Property(i => i.UnitPrice).HasPrecision(10, 2);

        modelBuilder.Entity<Order>(e =>
        {
            e.HasIndex(o => o.Code).IsUnique();
            e.Property(o => o.Status).HasConversion<string>();
            e.HasIndex(o => o.Status).IsUnique().HasFilter("[Status] = 'InProgress'");
        });
    }
}