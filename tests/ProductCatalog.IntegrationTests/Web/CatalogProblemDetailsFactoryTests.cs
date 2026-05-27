using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using ProductCatalog.Application.Common.Exceptions;
using ProductCatalog.Web.Services;

namespace ProductCatalog.IntegrationTests.Web;

public sealed class CatalogProblemDetailsFactoryTests
{
    [Fact]
    public void CreateForException_ShouldIncludeDetail_InDevelopment()
    {
        var factory = new CatalogProblemDetailsFactory(new FakeHostEnvironment("Development"));

        var problemDetails = factory.CreateForException(
            "/api/products/123",
            "trace-dev",
            new NotFoundException("No se encontro el producto '123'."));

        problemDetails.Title.Should().Be("Producto no encontrado");
        problemDetails.Detail.Should().Be("No se encontro el producto '123'.");
        ReadExtension(problemDetails, "correlationId").Should().Be("trace-dev");
    }

    [Fact]
    public void CreateForException_ShouldHideDetail_OutsideDevelopment()
    {
        var factory = new CatalogProblemDetailsFactory(new FakeHostEnvironment("Production"));

        var problemDetails = factory.CreateForException(
            "/api/products/123",
            "trace-prod",
            new NotFoundException("No se encontro el producto '123'."));

        problemDetails.Title.Should().Be("Producto no encontrado");
        problemDetails.Detail.Should().Be("No se encontro el producto solicitado.");
        ReadExtension(problemDetails, "correlationId").Should().Be("trace-prod");
    }

    private static string? ReadExtension(ProblemDetails problemDetails, string key)
    {
        problemDetails.Extensions.TryGetValue(key, out var value);
        return value?.ToString();
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; } = "ProductCatalog.Web";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
