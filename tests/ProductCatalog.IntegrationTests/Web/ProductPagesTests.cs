using System.Net;
using System.Net.Http.Json;

namespace ProductCatalog.IntegrationTests.Web;

public sealed class ProductPagesTests
{
    [Fact]
    public async Task ProductsPage_ShouldRenderListingToolsAndMetadata()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/products");

        html.Should().Contain("Products");
        html.Should().Contain("Search by name or SKU");
        html.Should().Contain("Source");
        html.Should().MatchRegex("Database|Cache");
    }

    [Fact]
    public async Task CreatePage_ShouldRenderValidationAndRules()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/products/new");

        html.Should().Contain("Create product");
        html.Should().Contain("Field validation runs in browser");
        html.Should().Contain("SKU required and unique");
    }

    [Fact]
    public async Task EditPage_ShouldRenderLoadedProductSnapshot()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();

        using var createResponse = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = "Edit Screen Product",
                Sku = "UI-EDIT-001",
                SalePrice = 215m,
                Cost = 180m,
                Stock = 8
            });

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ProductCatalog.Application.Products.Dtos.ProductDto>();
        created.Should().NotBeNull();

        var html = await client.GetStringAsync($"/products/{created!.Id:D}/edit");

        html.Should().Contain("Edit product");
        html.Should().Contain("Edit Screen Product");
        html.Should().Contain("Original stock");
    }

    [Fact]
    public async Task EditPage_ShouldRenderProblemDetailsTitle_WhenProductDoesNotExist()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();
        var missingId = Guid.NewGuid();

        var html = await client.GetStringAsync($"/products/{missingId:D}/edit");

        html.Should().Contain("Product not found");
        html.Should().NotContain($"Product '{missingId:D}' was not found.");
    }
}
