using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ProductCatalog.Infrastructure.Persistence;
using ProductCatalog.Infrastructure.Seeding;

namespace ProductCatalog.Infrastructure.Startup;

public static class DatabaseInitializationExtensions
{
    private const int DatabaseAlreadyExistsErrorNumber = 1801;
    private const int LoginFailedForDatabaseErrorNumber = 4060;

    public static async Task InitializeCatalogDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        const int maxAttempts = 3;
        var delay = TimeSpan.FromSeconds(5);
        Exception? lastException = null;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("ProductCatalog.DatabaseInitialization");
            var seeder = scope.ServiceProvider.GetRequiredService<ProductCatalogSeeder>();

            try
            {
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
                return;
            }
            catch (SqlException exception) when (attempt < maxAttempts && IsRecoverableStartupException(exception))
            {
                lastException = exception;
                logger.LogWarning(
                    exception,
                    "Database initialization attempt {Attempt} failed with transient startup condition. Retrying in {DelaySeconds}s.",
                    attempt,
                    delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("Database initialization failed after retries.");
    }

    private static bool IsRecoverableStartupException(SqlException exception)
    {
        return exception.Number is DatabaseAlreadyExistsErrorNumber or LoginFailedForDatabaseErrorNumber;
    }
}
