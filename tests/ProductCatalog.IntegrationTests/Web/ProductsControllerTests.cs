using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.IntegrationTests.Web;

public sealed class ProductsControllerTests
{
    [Fact]
    public async Task Post_ShouldReturn422ProblemDetails_WhenDomainInvariantFails()
    {
        await using var factory = new ProductCatalogApiFactory();
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

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        problemDetails!.Title.Should().Contain("Invalid price");
    }

    [Fact]
    public async Task ErrorResponses_ShouldEchoIncomingCorrelationId()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();
        var correlationId = Guid.NewGuid().ToString("D");
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/products/{Guid.NewGuid():D}");
        request.Headers.Add("X-Correlation-Id", correlationId);

        using var response = await client.SendAsync(request);
        using var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Headers.GetValues("X-Correlation-Id").Single().Should().Be(correlationId);
        payload.RootElement.GetProperty("correlationId").GetString().Should().Be(correlationId);
        payload.RootElement.GetProperty("traceId").GetString().Should().Be(correlationId);
    }

    [Fact]
    public async Task GetById_ShouldReturn404ProblemDetails_WhenProductDoesNotExist()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"/api/products/{Guid.NewGuid():D}");
        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        problemDetails!.Title.Should().Contain("Product not found");
    }

    [Fact]
    public async Task Post_ShouldReturn400ProblemDetails_WhenValidationFails()
    {
        await using var factory = new ProductCatalogApiFactory();
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

        var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        problemDetails!.Title.Should().Contain("Validation failed");
    }

    [Fact]
    public async Task SuccessResponses_ShouldGenerateCorrelationId_WhenRequestHeaderIsMissing()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/products?pageNumber=1&pageSize=1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Contains("X-Correlation-Id").Should().BeTrue();
        response.Headers.GetValues("X-Correlation-Id").Single().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetProducts_ShouldReturnPagedProducts_FromSeededDatabase()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<PagedResult<ProductDto>>("/api/products?pageNumber=1&pageSize=5&sortField=Name&sortDirection=Asc");

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(5);
        result.Metadata.Should().NotBeNull();
    }

    [Fact]
    public async Task CrudAndSkuExistsEndpoints_ShouldWork_EndToEnd()
    {
        await using var factory = new ProductCatalogApiFactory();
        using var client = factory.CreateClient();
        var requestId = Guid.NewGuid();

        using var createResponse = await client.PostAsJsonAsync(
            "/api/products",
            new
            {
                Name = "API Created Product",
                Sku = "API-CRUD-001",
                SalePrice = 155m,
                Cost = 120m,
                Stock = 6
            },
            cancellationToken: CancellationToken.None);

        createResponse.Headers.Location.Should().NotBeNull();
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductDto>();
        created.Should().NotBeNull();

        var skuExists = await client.GetFromJsonAsync<SkuExistsDto>("/api/products/sku-exists?sku=api--crud-001");
        skuExists.Should().NotBeNull();
        skuExists!.Exists.Should().BeTrue();
        skuExists.Sku.Should().Be("API-CRUD-001");

        using var updateRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/products/{created!.Id:D}")
        {
            Content = JsonContent.Create(new
            {
                SalePrice = 180m,
                Cost = 120m,
                StockDelta = 4
            })
        };
        updateRequest.Headers.Add("X-Request-Id", requestId.ToString());
        using var updateResponse = await client.SendAsync(updateRequest);
        var updated = await updateResponse.Content.ReadFromJsonAsync<ProductDto>();

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        updated!.SalePrice.Should().Be(180m);
        updated.Stock.Should().Be(10);

        using var deleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/products/{created.Id:D}");
        deleteRequest.Headers.Add("X-Request-Id", Guid.NewGuid().ToString());
        using var deleteResponse = await client.SendAsync(deleteRequest);

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var getDeletedResponse = await client.GetAsync($"/api/products/{created.Id:D}");
        getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RouteTable_ShouldExposeProductControllerEndpoints_WithoutMinimalApiHandlers()
    {
        await using var factory = new ProductCatalogApiFactory();
        _ = factory.CreateClient();

        var dataSource = factory.Services.GetRequiredService<EndpointDataSource>();
        var productEndpoints = dataSource.Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText is not null)
            .Where(endpoint => endpoint.RoutePattern.RawText!.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
            .Where(endpoint => endpoint.RoutePattern.RawText!.Contains("products", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        productEndpoints.Should().NotBeEmpty();
        productEndpoints.Select(endpoint => endpoint.RoutePattern.RawText!.ToLowerInvariant())
            .Should()
            .Contain(route => route == "api/products");
        productEndpoints.Should().OnlyContain(endpoint =>
            endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() != null);
    }
}
