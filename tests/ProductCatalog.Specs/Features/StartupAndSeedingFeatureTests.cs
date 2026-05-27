using System.Net.Http.Json;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Specs.Web;

namespace ProductCatalog.Specs.Features;

public sealed class StartupAndSeedingFeatureTests
{
    [Fact(DisplayName = "BDD-017 Seed catalog when database is empty")]
    [Trait("ScenarioId", "BDD-017")]
    public async Task Bdd017_SeedCatalogWhenDatabaseIsEmpty()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<PagedResult<ProductDto>>(
            "/api/products?pageNumber=1&pageSize=25&sortField=Name&sortDirection=Asc");

        Assert.NotNull(result);
        Assert.Equal(20, result!.TotalCount);
    }

    [Fact(DisplayName = "BDD-018 Avoid duplicate seeding on restart")]
    [Trait("ScenarioId", "BDD-018")]
    public async Task Bdd018_AvoidDuplicateSeedingOnRestart()
    {
        var databasePath = Path.Combine(Path.GetTempPath(), $"productcatalog-bdd-restart-{Guid.NewGuid():N}.db");

        try
        {
            await using (var firstFactory = new ProductCatalogBddFactory(databasePath))
            {
                using var firstClient = firstFactory.CreateClient();
                var firstResult = await firstClient.GetFromJsonAsync<PagedResult<ProductDto>>(
                    "/api/products?pageNumber=1&pageSize=25&sortField=Name&sortDirection=Asc");

                Assert.NotNull(firstResult);
                Assert.Equal(20, firstResult!.TotalCount);
            }

            await using (var secondFactory = new ProductCatalogBddFactory(databasePath))
            {
                using var secondClient = secondFactory.CreateClient();
                var secondResult = await secondClient.GetFromJsonAsync<PagedResult<ProductDto>>(
                    "/api/products?pageNumber=1&pageSize=25&sortField=Name&sortDirection=Asc");

                Assert.NotNull(secondResult);
                Assert.Equal(20, secondResult!.TotalCount);
            }
        }
        finally
        {
            try
            {
                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                }
            }
            catch (IOException)
            {
            }
        }
    }
}
