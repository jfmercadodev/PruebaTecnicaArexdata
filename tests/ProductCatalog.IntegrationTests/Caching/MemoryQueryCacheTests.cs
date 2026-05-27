using ProductCatalog.Infrastructure.Caching;

namespace ProductCatalog.IntegrationTests.Caching;

public sealed class MemoryQueryCacheTests
{
    [Fact]
    public async Task InvalidateByPrefixAsync_ShouldRemoveMatchingEntries()
    {
        var cache = new MemoryQueryCache(new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions()));

        await cache.SetAsync("products:p1", "value-1", TimeSpan.FromMinutes(5));
        await cache.SetAsync("products:p2", "value-2", TimeSpan.FromMinutes(5));
        await cache.SetAsync("other:key", "value-3", TimeSpan.FromMinutes(5));

        await cache.InvalidateByPrefixAsync("products:");

        (await cache.GetAsync<string>("products:p1")).Should().BeNull();
        (await cache.GetAsync<string>("products:p2")).Should().BeNull();
        (await cache.GetAsync<string>("other:key")).Should().Be("value-3");
    }
}
