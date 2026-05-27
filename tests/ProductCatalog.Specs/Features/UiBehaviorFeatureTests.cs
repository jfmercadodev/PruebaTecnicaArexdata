using System.Diagnostics;
using Bunit;
using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Application.Products.Queries.CheckSkuExists;
using ProductCatalog.Web.Components;
using ProductCatalog.Web.Components.Pages;
using ProductCatalog.Web.Services;

namespace ProductCatalog.Specs.Features;

public sealed class UiBehaviorFeatureTests
{
    [Fact(DisplayName = "BDD-021 Block invalid create form until SKU validation passes")]
    [Trait("ScenarioId", "BDD-021")]
    public void Bdd021_BlockInvalidCreateFormUntilSkuValidationPasses()
    {
        using var context = new TestContext();
        context.Services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment("Development"));
        context.Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        });
        context.Services.AddSingleton(new CatalogProblemDetailsFactory(new FakeHostEnvironment("Development")));
        context.Services.AddSingleton<ISender>(new FakeSender(request =>
        {
            return request switch
            {
                CheckSkuExistsQuery => Task.FromResult<object?>(new SkuExistsDto("MKB-001", true)),
                _ => throw new InvalidOperationException($"Unexpected request type {request.GetType().Name}.")
            };
        }));
        context.Services.AddScoped<ProductCatalogFacade>();

        var cut = context.RenderComponent<ProductCreate>();

        cut.Find("#name").Change("Mechanical Keyboard");
        cut.Find("#sku").Change("mkb-001");
        cut.Find("#sale-price").Change("120");
        cut.Find("#cost").Change("70");
        cut.Find("#stock").Change("15");

        cut.WaitForAssertion(
            () => Assert.Contains("SKU already exists.", cut.Markup),
            TimeSpan.FromSeconds(2));

        Assert.True(cut.Find("button[type=submit]").HasAttribute("disabled"));
    }

    [Fact(DisplayName = "BDD-023 Show technical detail in Development error boundary")]
    [Trait("ScenarioId", "BDD-023")]
    public void Bdd023_ShowTechnicalDetailInDevelopmentErrorBoundary()
    {
        using var context = new TestContext();
        context.Services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment("Development"));

        var cut = context.RenderComponent<CustomErrorBoundary>(parameters =>
            parameters.Add(boundary => boundary.ChildContent, ThrowingFragment("boom-dev")));

        cut.WaitForAssertion(
            () => Assert.Contains("boom-dev", cut.Markup),
            TimeSpan.FromSeconds(1));

        Assert.Contains("Screen failed", cut.Markup);
        Assert.Contains("System.InvalidOperationException", cut.Markup);
    }

    [Fact(DisplayName = "BDD-024 Show friendly message in Production error boundary")]
    [Trait("ScenarioId", "BDD-024")]
    public void Bdd024_ShowFriendlyMessageInProductionErrorBoundary()
    {
        using var activity = new Activity("bdd-boundary");
        activity.Start();

        try
        {
            using var context = new TestContext();
            context.Services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment("Production"));

            var cut = context.RenderComponent<CustomErrorBoundary>(parameters =>
                parameters.Add(boundary => boundary.ChildContent, ThrowingFragment("boom-prod")));

            cut.WaitForAssertion(
                () => Assert.Contains("Something broke while rendering this screen.", cut.Markup),
                TimeSpan.FromSeconds(1));

            Assert.Contains("Correlation:", cut.Markup);
            Assert.DoesNotContain("boom-prod", cut.Markup);
        }
        finally
        {
            activity.Stop();
        }
    }

    private static RenderFragment ThrowingFragment(string message)
    {
        return builder =>
        {
            builder.OpenComponent<ThrowingComponent>(0);
            builder.AddAttribute(1, nameof(ThrowingComponent.Message), message);
            builder.CloseComponent();
        };
    }

    private sealed class ThrowingComponent : ComponentBase
    {
        [Parameter]
        public string Message { get; set; } = string.Empty;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            throw new InvalidOperationException(Message);
        }
    }

    private sealed class FakeSender : ISender
    {
        private readonly Func<object, Task<object?>> _handler;

        public FakeSender(Func<object, Task<object?>> handler)
        {
            _handler = handler;
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            return (TResponse)(await _handler(request) ?? throw new InvalidOperationException("Null response."));
        }

        public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
        {
            _ = await _handler(request);
        }

        public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
        {
            return await _handler(request);
        }

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }

        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
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
