using Microsoft.EntityFrameworkCore;
using ProductCatalog.Infrastructure.Seeding;

namespace ProductCatalog.IntegrationTests.Seeding;

public sealed class ProductCatalogSeederTests
{
    [Fact]
    public async Task SeedAsync_ShouldInsertAtLeastTwentyProducts_WhenDatabaseIsEmpty()
    {
        using var scope = ApplicationDbContextScope.Create();
        var seeder = new ProductCatalogSeeder();

        var insertedRows = await seeder.SeedAsync(scope.DbContext);

        var products = await scope.DbContext.Products
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .ToArrayAsync();

        insertedRows.Should().BeGreaterThanOrEqualTo(20);
        products.Should().HaveCountGreaterThanOrEqualTo(20);
        products.Select(product => product.Sku.Value).Should().OnlyHaveUniqueItems();
        products.Select(product => product.SalePrice.Value).Distinct().Count().Should().BeGreaterThan(5);
        products.Select(product => product.Cost.Value).Distinct().Count().Should().BeGreaterThan(5);
        products.Select(product => product.Stock).Distinct().Count().Should().BeGreaterThan(5);
    }

    [Fact]
    public async Task SeedAsync_ShouldNotDuplicateProducts_WhenDatabaseAlreadyContainsRows()
    {
        using var scope = ApplicationDbContextScope.Create();
        var seeder = new ProductCatalogSeeder();

        await seeder.SeedAsync(scope.DbContext);
        var countAfterFirstRun = await scope.DbContext.Products.CountAsync();

        var insertedRows = await seeder.SeedAsync(scope.DbContext);
        var countAfterSecondRun = await scope.DbContext.Products.CountAsync();

        insertedRows.Should().Be(0);
        countAfterSecondRun.Should().Be(countAfterFirstRun);
    }
}
