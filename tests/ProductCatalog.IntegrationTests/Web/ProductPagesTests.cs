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

        html.Should().Contain("Productos");
        html.Should().Contain("Buscar por nombre o SKU");
        html.Should().Contain("Fuente");
        html.Should().MatchRegex("Base de datos|Cache");
    }

    [Fact]
    public async Task CreatePage_ShouldRenderValidationAndRules()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();

        var html = await client.GetStringAsync("/products/new");

        html.Should().Contain("Crear producto");
        html.Should().Contain("La validacion corre en navegador");
        html.Should().Contain("SKU obligatorio y unico");
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

        html.Should().Contain("Editar producto");
        html.Should().Contain("Edit Screen Product");
        html.Should().Contain("edit-name");
        html.Should().Contain("edit-sku");
        html.Should().Contain("Stock original");
    }

    [Fact]
    public async Task EditPage_ShouldRenderProblemDetailsTitle_WhenProductDoesNotExist()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();
        var missingId = Guid.NewGuid();

        var html = await client.GetStringAsync($"/products/{missingId:D}/edit");

        html.Should().Contain("Producto no encontrado");
        html.Should().NotContain($"No se encontro el producto '{missingId:D}'.");
    }
}
