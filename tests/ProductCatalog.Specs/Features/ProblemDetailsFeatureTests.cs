using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Specs.Web;

namespace ProductCatalog.Specs.Features;

public sealed class ProblemDetailsFeatureTests
{
    [Fact(DisplayName = "BDD-012 Return 422 for domain invariant violation")]
    [Trait("ScenarioId", "BDD-012")]
    public async Task Bdd012_Return422ForDomainInvariantViolation()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = "Invalid Margin Keyboard",
                Sku = "KB-422-001",
                SalePrice = 40m,
                Cost = 55m,
                Stock = 5
            });

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Precio invalido", problem!.Title);
    }

    [Fact(DisplayName = "BDD-013 Return 404 for missing product")]
    [Trait("ScenarioId", "BDD-013")]
    public async Task Bdd013_Return404ForMissingProduct()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"/api/products/{Guid.NewGuid():D}");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Producto no encontrado", problem!.Title);
    }

    [Fact(DisplayName = "BDD-014 Return 400 for validation error")]
    [Trait("ScenarioId", "BDD-014")]
    public async Task Bdd014_Return400ForValidationError()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = string.Empty,
                Sku = string.Empty,
                SalePrice = 100m,
                Cost = 80m,
                Stock = 5
            });

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Validacion fallida", problem!.Title);
    }

    [Fact(DisplayName = "BDD-019 Propagate correlation id through response and errors")]
    [Trait("ScenarioId", "BDD-019")]
    public async Task Bdd019_PropagateCorrelationIdThroughResponseAndErrors()
    {
        await using var factory = new ProductCatalogBddFactory();
        using var client = factory.CreateClient();
        var correlationId = Guid.NewGuid().ToString("D");
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{Guid.NewGuid():D}");
        request.Headers.Add("X-Correlation-Id", correlationId);

        using var response = await client.SendAsync(request);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(correlationId, response.Headers.GetValues("X-Correlation-Id").Single());
        Assert.Equal(correlationId, payload.RootElement.GetProperty("correlationId").GetString());
    }
}
