using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Infrastructure.Persistence;

namespace ProductCatalog.IntegrationTests;

internal sealed class ApplicationDbContextScope : IDisposable
{
    private ApplicationDbContextScope(AppDbContext dbContext, SqliteConnection connection)
    {
        DbContext = dbContext;
        Connection = connection;
    }

    public AppDbContext DbContext { get; }

    public SqliteConnection Connection { get; }

    public static ApplicationDbContextScope Create()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new AppDbContext(options);
        dbContext.Database.EnsureCreated();
        return new ApplicationDbContextScope(dbContext, connection);
    }

    public void Dispose()
    {
        DbContext.Dispose();
        Connection.Dispose();
    }
}
