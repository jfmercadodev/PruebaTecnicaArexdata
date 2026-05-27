using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Application.Products.Mappings;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.UnitTests.Application.Mappings;

public sealed class ProductProfileTests
{
    [Fact]
    public void Configuration_ShouldBeValid()
    {
        var configuration = new MapperConfiguration(
            cfg => cfg.AddProfile<ProductProfile>(),
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void ProductContract_ShouldRequireMappingReview_WhenEntityShapeChanges()
    {
        var sourceMembers = typeof(Product)
            .GetProperties()
            .Select(property => property.Name)
            .OrderBy(name => name)
            .ToArray();

        sourceMembers.Should().Equal(
            "Cost",
            "DomainEvents",
            "Id",
            "MarginPercent",
            "Name",
            "SalePrice",
            "Sku",
            "Stock");
    }

    [Fact]
    public void Map_ShouldProject_AllExpectedDtoFields()
    {
        var mapper = MapperFactory.Create();
        var product = Product.Create(
            "Mechanical Keyboard",
            Sku.Create("mkb--001"),
            Money.Create(120m),
            Money.Create(70m),
            15);

        var dto = mapper.Map<ProductDto>(product);

        dto.Name.Should().Be("Mechanical Keyboard");
        dto.Sku.Should().Be("MKB-001");
        dto.SalePrice.Should().Be(120m);
        dto.Cost.Should().Be(70m);
        dto.Stock.Should().Be(15);
        dto.MarginPercent.Should().Be(41.6667m);
    }
}
