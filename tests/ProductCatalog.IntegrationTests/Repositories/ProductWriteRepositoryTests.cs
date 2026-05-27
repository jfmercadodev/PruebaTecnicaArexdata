using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.ValueObjects;
using ProductCatalog.Infrastructure.Repositories;

namespace ProductCatalog.IntegrationTests.Repositories;

public sealed class ProductWriteRepositoryTests
{
    [Fact]
    public async Task Delete_ShouldSoftDelete_Product()
    {
        using var scope = ApplicationDbContextScope.Create();
        var product = CreateProduct("Mechanical Keyboard", "MKB-001", 15);
        scope.DbContext.Products.Add(product);
        await scope.DbContext.SaveChangesAsync();

        var repository = new ProductWriteRepository(scope.DbContext);
        repository.Delete(product);
        await scope.DbContext.SaveChangesAsync();

        var visibleProduct = await scope.DbContext.Products.SingleOrDefaultAsync(candidate => candidate.Id == product.Id);
        visibleProduct.Should().BeNull();
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
