using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.UnitTests.Domain.Entities;

public sealed class ProductTests
{
    [Fact]
    public void Create_ShouldRaiseProductCreated_WithNormalizedState()
    {
        var product = Product.Create(
            "Mechanical Keyboard",
            Sku.Create("mkb--001"),
            Money.Create(120m),
            Money.Create(70m),
            15);

        product.Sku.Value.Should().Be("MKB-001");
        product.MarginPercent.Should().Be(41.6667m);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreated>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenSalePriceIsLowerThanCost()
    {
        var action = () => Product.Create(
            "Gaming Mouse",
            Sku.Create("GM-001"),
            Money.Create(40m),
            Money.Create(55m),
            10);

        action.Should().Throw<InvalidPriceException>();
    }

    [Fact]
    public void Create_ShouldThrow_WhenNameIsInvalid()
    {
        var action = () => Product.Create(
            "  ",
            Sku.Create("GM-001"),
            Money.Create(60m),
            Money.Create(55m),
            10);

        action.Should().Throw<InvalidProductNameException>();
    }

    [Fact]
    public void UpdatePrice_ShouldChangeValues_AndRaiseSingleUpdatedEvent()
    {
        var product = CreatePersistedProduct();

        product.UpdatePrice(135m, 80m);

        product.SalePrice.Value.Should().Be(135m);
        product.Cost.Value.Should().Be(80m);
        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductUpdated>();
    }

    [Fact]
    public void MultipleUpdates_ShouldKeepSingleRepresentativeUpdatedEvent()
    {
        var product = CreatePersistedProduct();

        product.UpdatePrice(125m, 75m);
        product.AdjustStock(-2);

        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductUpdated>();
    }

    [Fact]
    public void CreateThenUpdateWithoutClearingEvents_ShouldKeepSingleCreatedEvent()
    {
        var product = Product.Create(
            "Mechanical Keyboard",
            Sku.Create("mkb--001"),
            Money.Create(120m),
            Money.Create(70m),
            15);

        product.UpdatePrice(135m, 80m);
        product.AdjustStock(-2);

        product.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreated>();
    }

    [Fact]
    public void AdjustStock_ShouldThrow_WhenStockWouldBeNegative()
    {
        var product = CreatePersistedProduct(stock: 2);

        var action = () => product.AdjustStock(-3);

        action.Should().Throw<InvalidStockException>();
        product.Stock.Should().Be(2);
    }

    [Fact]
    public void ExecuteAtomic_ShouldRollbackStateAndEvents_WhenCompositeChangeFails()
    {
        var product = CreatePersistedProduct();

        var action = () => product.ExecuteAtomic(candidate =>
        {
            candidate.AdjustStock(-4);
            candidate.UpdatePrice(60m, 80m);
        });

        action.Should().Throw<InvalidPriceException>();
        product.SalePrice.Value.Should().Be(120m);
        product.Cost.Value.Should().Be(70m);
        product.Stock.Should().Be(15);
        product.DomainEvents.Should().BeEmpty();
    }

    private static Product CreatePersistedProduct(int stock = 15)
    {
        var product = Product.Create(
            "Mechanical Keyboard",
            Sku.Create("mkb--001"),
            Money.Create(120m),
            Money.Create(70m),
            stock);

        product.ClearDomainEvents();
        return product;
    }
}
