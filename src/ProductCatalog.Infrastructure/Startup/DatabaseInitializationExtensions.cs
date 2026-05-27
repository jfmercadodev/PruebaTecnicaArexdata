using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductCatalog.Infrastructure.Persistence;
using ProductCatalog.Infrastructure.Seeding;

namespace ProductCatalog.Infrastructure.Startup;

public static class DatabaseInitializationExtensions
{
    public static async Task InitializeCatalogDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("ProductCatalog.DatabaseInitialization");
        var seeder = scope.ServiceProvider.GetRequiredService<ProductCatalogSeeder>();

        var hasMigrations = dbContext.Database.IsSqlServer() && dbContext.Database.GetMigrations().Any();
        if (hasMigrations)
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migrations applied.");
        }
        else
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
            logger.LogInformation("Database schema created without migrations.");
        }

        var insertedRows = await seeder.SeedAsync(dbContext, cancellationToken);
        logger.LogInformation("Database seeding finished. Inserted rows: {InsertedRows}", insertedRows);
    }
}
