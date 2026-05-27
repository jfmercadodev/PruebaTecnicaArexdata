using ProductCatalog.Application.Common.Enums;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Specifications;
using ProductCatalog.Domain.ValueObjects;
using ProductCatalog.Infrastructure.Repositories;

namespace ProductCatalog.IntegrationTests.Repositories;

public sealed class ProductReadRepositoryTests
{
    [Fact]
    public async Task ExistsBySkuAsync_ShouldFindNormalizedSku()
    {
        using var scope = ApplicationDbContextScope.Create();
        var product = CreateProduct("Mechanical Keyboard", "mkb--001", 15);
        scope.DbContext.Products.Add(product);
        await scope.DbContext.SaveChangesAsync();

        var repository = new ProductReadRepository(scope.DbContext);

        var exists = await repository.ExistsBySkuAsync(Sku.Create("MKB-001"));

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldApplyFilterSortingAndPaging()
    {
        using var scope = ApplicationDbContextScope.Create();
        scope.DbContext.Products.AddRange(
            CreateProduct("Gamma", "SKU-003", 3),
            CreateProduct("Alpha", "SKU-001", 10),
            CreateProduct("Beta", "SKU-002", 2));
        await scope.DbContext.SaveChangesAsync();

        var repository = new ProductReadRepository(scope.DbContext);
        var filter = new ActiveProductSpecification() & new LowStockSpecification(5);

        var page = await repository.GetPagedAsync(1, 10, "Name", SortDirection.Asc, filter);

        page.TotalCount.Should().Be(2);
        page.Items.Select(product => product.Name).Should().Equal("Beta", "Gamma");
    }

    private static Product CreateProduct(string name, string sku, int stock)
    {
        var product = Product.Create(
            name,
            Sku.Create(sku),
            Money.Create(120m),
            Money.Create(70m),
            stock);
        product.ClearDomainEvents();
        return product;
    }
}
