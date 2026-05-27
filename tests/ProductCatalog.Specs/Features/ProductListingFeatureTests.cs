using System.Net.Http.Json;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Specs.Web;

namespace ProductCatalog.Specs.Features;

public sealed class ProductListingFeatureTests
{
    [Fact(DisplayName = "BDD-008 List products with paging and sorting")]
    [Trait("ScenarioId", "BDD-008")]
    public async Task Bdd008_ListProductsWithPagingAndSorting()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<PagedResult<ProductDto>>(
            "/api/products?pageNumber=2&pageSize=10&sortField=Name&sortDirection=Asc");

        Assert.NotNull(result);
        Assert.Equal(10, result!.Items.Count);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);

        var names = result.Items.Select(product => product.Name).ToArray();
        var expected = names.OrderBy(name => name, StringComparer.Ordinal).ToArray();
        Assert.Equal(expected, names);
    }

    [Fact(DisplayName = "BDD-009 Filter products by name or sku")]
    [Trait("ScenarioId", "BDD-009")]
    public async Task Bdd009_FilterProductsByNameOrSku()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<PagedResult<ProductDto>>(
            "/api/products?pageNumber=1&pageSize=10&sortField=Name&sortDirection=Asc&searchTerm=KB-001");

        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal("Mechanical Keyboard", result.Items.Single().Name);
    }

    [Fact(DisplayName = "BDD-010 Show cache source metadata")]
    [Trait("ScenarioId", "BDD-010")]
    public async Task Bdd010_ShowCacheSourceMetadata()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();
        const string query = "/api/products?pageNumber=1&pageSize=5&sortField=Name&sortDirection=Asc&searchTerm=M";

        var first = await client.GetFromJsonAsync<PagedResult<ProductDto>>(query);
        var second = await client.GetFromJsonAsync<PagedResult<ProductDto>>(query);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.Equal("Database", first!.Metadata!.Source);
        Assert.Equal("Cache", second!.Metadata!.Source);
        Assert.True(second.Metadata.ElapsedMs >= 0);
    }

    [Fact(DisplayName = "BDD-011 Invalidate cache after write")]
    [Trait("ScenarioId", "BDD-011")]
    public async Task Bdd011_InvalidateCacheAfterWrite()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();
        const string query = "/api/products?pageNumber=1&pageSize=5&sortField=Name&sortDirection=Asc";

        _ = await client.GetFromJsonAsync<PagedResult<ProductDto>>(query);
        var cached = await client.GetFromJsonAsync<PagedResult<ProductDto>>(query);

        using var createResponse = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = "Cache Breaker",
                Sku = "CB-001",
                SalePrice = 111m,
                Cost = 80m,
                Stock = 4
            });

        var afterWrite = await client.GetFromJsonAsync<PagedResult<ProductDto>>(query);

        Assert.NotNull(cached);
        Assert.NotNull(afterWrite);
        Assert.Equal("Cache", cached!.Metadata!.Source);
        Assert.Equal("Database", afterWrite!.Metadata!.Source);
    }
}
