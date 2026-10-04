using AwesomePizza.Api.Data;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AwesomePizza.Tests.Integration;

// Starts the real API in memory against a separate SQL Server database, never the development one.
// In Development the API migrates and seeds the database on startup, so the test database is ready on its own.
public class ApiFactory : WebApplicationFactory<Program>
{
    private const string TestDatabaseName = "AwesomePizza_Tests";

    private string _connectionString = string.Empty;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices((context, services) =>
        {
            // Same server and credentials of the development connection string, only the database changes
            SqlConnectionStringBuilder connectionStringBuilder =
                new(context.Configuration.GetConnectionString("AwesomePizza"));

            if (connectionStringBuilder.InitialCatalog == TestDatabaseName)
            {
                throw new InvalidOperationException("The test database name must differ from the configured one.");
            }

            connectionStringBuilder.InitialCatalog = TestDatabaseName;
            _connectionString = connectionStringBuilder.ConnectionString;

            services.RemoveAll<IDbContextOptionsConfiguration<DatabaseContext>>();
            services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(_connectionString));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        // The host is stopped: drop the test database so no state is left between runs
        DbContextOptions<DatabaseContext> options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlServer(_connectionString)
            .Options;

        await using DatabaseContext context = new(options);
        await context.Database.EnsureDeletedAsync();
    }
}
