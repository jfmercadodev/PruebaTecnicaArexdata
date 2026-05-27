using MediatR;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Infrastructure.Idempotency;

namespace ProductCatalog.IntegrationTests.Idempotency;

public sealed class EfIdempotencyStoreTests
{
    [Fact]
    public async Task SaveAndGetAsync_ShouldRoundTripProductDto()
    {
        using var scope = ApplicationDbContextScope.Create();
        var store = new EfIdempotencyStore(scope.DbContext);
        var requestId = Guid.NewGuid();
        var dto = new ProductDto(Guid.NewGuid(), "Mechanical Keyboard", "MKB-001", 120m, 70m, 15, 41.6667m);

        await store.SaveAsync(requestId, dto, TimeSpan.FromMinutes(10));
        await scope.DbContext.SaveChangesAsync();
        var loaded = await store.GetAsync<ProductDto>(requestId);

        loaded.Should().NotBeNull();
        loaded!.Response.Should().Be(dto);
    }

    [Fact]
    public async Task SaveAndGetAsync_ShouldRoundTripUnit()
    {
        using var scope = ApplicationDbContextScope.Create();
        var store = new EfIdempotencyStore(scope.DbContext);
        var requestId = Guid.NewGuid();

        await store.SaveAsync(requestId, Unit.Value, TimeSpan.FromMinutes(10));
        await scope.DbContext.SaveChangesAsync();
        var loaded = await store.GetAsync<Unit>(requestId);

        loaded.Should().NotBeNull();
    }
}
