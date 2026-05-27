using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Infrastructure.Persistence;

namespace ProductCatalog.Specs.Web;

public sealed class ProductCatalogBddFactory : WebApplicationFactory<ProductCatalog.Web.Program>
{
    private const string TestConnectionString = "Server=localhost,1433;Database=ProductCatalogDb;User Id=sa;Password=SqlServerDev123!;TrustServerCertificate=True;Encrypt=False";
    private readonly string? _originalConnectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

    public ProductCatalogBddFactory(string? databasePath = null)
    {
        DatabasePath = databasePath ?? Path.Combine(Path.GetTempPath(), $"productcatalog-bdd-{Guid.NewGuid():N}.db");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", TestConnectionString);
    }

    public string DatabasePath { get; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.ClearProviders());
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>(
                    "ConnectionStrings:DefaultConnection",
                    TestConnectionString),
                new KeyValuePair<string, string?>(
                    "Serilog:WriteTo:0:Name",
                    "Console"),
                new KeyValuePair<string, string?>(
                    "Serilog:WriteTo:0:Args:outputTemplate",
                    "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            ]);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<IUnitOfWork>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={DatabasePath}"));

            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _originalConnectionString);
        }
    }
}
