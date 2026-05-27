using System.Net;
using System.Net.Http.Json;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Specs.Web;

namespace ProductCatalog.Specs.Features;

public sealed class ProductCreationFeatureTests
{
    [Fact(DisplayName = "BDD-001 Create valid product")]
    [Trait("ScenarioId", "BDD-001")]
    public async Task Bdd001_CreateValidProduct()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = "Mechanical Keyboard",
                Sku = "mkb--001",
                SalePrice = 120m,
                Cost = 70m,
                Stock = 15
            });

        var product = await response.Content.ReadFromJsonAsync<ProductDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(product);
        Assert.Equal("MKB-001", product!.Sku);
        Assert.Equal(41.6667m, decimal.Round(product.MarginPercent, 4));
    }

    [Fact(DisplayName = "BDD-002 Reject sale price below cost")]
    [Trait("ScenarioId", "BDD-002")]
    public async Task Bdd002_RejectSalePriceBelowCost()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = "Gaming Mouse",
                Sku = "gm-001",
                SalePrice = 40m,
                Cost = 55m,
                Stock = 10
            });

        var payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains("Invalid price", payload, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "BDD-003 Reject duplicate sku")]
    [Trait("ScenarioId", "BDD-003")]
    public async Task Bdd003_RejectDuplicateSku()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();

        _ = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = "Keyboard Prime",
                Sku = "MKB-001",
                SalePrice = 120m,
                Cost = 70m,
                Stock = 15
            });

        using var duplicateResponse = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = "Keyboard Clone",
                Sku = "mkb-001",
                SalePrice = 100m,
                Cost = 50m,
                Stock = 3
            });

        var payload = await duplicateResponse.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        Assert.Contains("Conflict detected", payload, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "BDD-004 Reuse idempotent response")]
    [Trait("ScenarioId", "BDD-004")]
    public async Task Bdd004_ReuseIdempotentResponse()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();
        var requestId = Guid.NewGuid().ToString("D");

        using var firstRequest = BuildCreateRequest(requestId);
        using var firstResponse = await client.SendAsync(firstRequest);
        var firstProduct = await firstResponse.Content.ReadFromJsonAsync<ProductDto>();

        using var secondRequest = BuildCreateRequest(requestId);
        using var secondResponse = await client.SendAsync(secondRequest);
        var secondProduct = await secondResponse.Content.ReadFromJsonAsync<ProductDto>();

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondResponse.StatusCode);
        Assert.NotNull(firstProduct);
        Assert.NotNull(secondProduct);
        Assert.Equal(firstProduct!.Id, secondProduct!.Id);
        Assert.Equal(firstProduct.Sku, secondProduct.Sku);
    }

    private static HttpRequestMessage BuildCreateRequest(string requestId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/products")
        {
            Content = JsonContent.Create(new
            {
                Name = "Idempotent Product",
                Sku = "IDM-001",
                SalePrice = 130m,
                Cost = 80m,
                Stock = 9
            })
        };

        request.Headers.Add("X-Request-Id", requestId);
        return request;
    }
}
