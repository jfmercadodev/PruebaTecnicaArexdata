using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Specifications;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.UnitTests.Domain.Specifications;

public sealed class ProductSpecificationTests
{
    [Fact]
    public void ActiveProductSpecification_ShouldMatch_WhenStockIsGreaterThanZero()
    {
        var product = CreateProduct(stock: 2);
        var specification = new ActiveProductSpecification();

        specification.IsSatisfiedBy(product).Should().BeTrue();
    }

    [Fact]
    public void LowStockSpecification_ShouldMatch_WhenStockIsBelowThreshold()
    {
        var product = CreateProduct(stock: 2);
        var specification = new LowStockSpecification(5);

        specification.IsSatisfiedBy(product).Should().BeTrue();
    }

    [Fact]
    public void CompositeSpecifications_ShouldSupportAndOrAndNotOperators()
    {
        var product = CreateProduct(stock: 2);
        var active = new ActiveProductSpecification();
        var lowStock = new LowStockSpecification(5);

        var andSpecification = active && lowStock;
        var orSpecification = active || new LowStockSpecification(1);
        var notSpecification = !new LowStockSpecification(2);

        andSpecification.IsSatisfiedBy(product).Should().BeTrue();
        orSpecification.IsSatisfiedBy(product).Should().BeTrue();
        notSpecification.IsSatisfiedBy(product).Should().BeTrue();
    }

    private static Product CreateProduct(int stock)
    {
        var product = Product.Create(
            "Mechanical Keyboard",
            Sku.Create("MKB-001"),
            Money.Create(120m),
            Money.Create(70m),
            stock);

        product.ClearDomainEvents();
        return product;
    }
}
