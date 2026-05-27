using MediatR;
using ProductCatalog.Application.Common.Abstractions;
using ProductCatalog.Application.Common.Behaviors;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Common.Models;

namespace ProductCatalog.UnitTests.Application.Behaviors;

public sealed class CachingBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldReturnCachedResponse_WhenCacheHit()
    {
        var cache = new FakeQueryCache();
        var cachedResponse = new FakeCacheResponse("cached");
        await cache.SetAsync("products:key", cachedResponse, TimeSpan.FromMinutes(5), CancellationToken.None);
        var behavior = new CachingBehavior<FakeCacheQuery, FakeCacheResponse>(cache);

        var response = await behavior.Handle(
            new FakeCacheQuery("products:key"),
            () => Task.FromResult(new FakeCacheResponse("fresh")),
            CancellationToken.None);

        response.Value.Should().Be("cached");
        response.Metadata!.CacheHit.Should().BeTrue();
        response.Metadata.Source.Should().Be("Cache");
    }

    [Fact]
    public async Task Handle_ShouldCacheResponse_WhenCacheMiss()
    {
        var cache = new FakeQueryCache();
        var behavior = new CachingBehavior<FakeCacheQuery, FakeCacheResponse>(cache);

        var response = await behavior.Handle(
            new FakeCacheQuery("products:key"),
            () => Task.FromResult(new FakeCacheResponse("fresh")),
            CancellationToken.None);

        response.Value.Should().Be("fresh");
        response.Metadata!.CacheHit.Should().BeFalse();
        response.Metadata.Source.Should().Be("Base de datos");
        (await cache.GetAsync<FakeCacheResponse>("products:key", CancellationToken.None))!.Value.Should().Be("fresh");
    }

    [Fact]
    public async Task Handle_ShouldBypassCache_WhenRequestIsNotCacheable()
    {
        var cache = new FakeQueryCache();
        var behavior = new CachingBehavior<FakePassthroughRequest, string>(cache);

        var response = await behavior.Handle(
            new FakePassthroughRequest(),
            () => Task.FromResult("passthrough"),
            CancellationToken.None);

        response.Should().Be("passthrough");
        cache.Keys.Should().BeEmpty();
    }

    private sealed record FakeCacheQuery(string CacheKey) : IRequest<FakeCacheResponse>, ICacheableQuery
    {
        public TimeSpan? Ttl => TimeSpan.FromMinutes(5);
    }

    private sealed record FakeCacheResponse(string Value, QueryMetadata? Metadata = null) : IMetadataAwareResponse<FakeCacheResponse>
    {
        public FakeCacheResponse WithMetadata(QueryMetadata metadata)
        {
            return this with { Metadata = metadata };
        }
    }

    private sealed record FakePassthroughRequest : IRequest<string>;

    private sealed class FakeQueryCache : IQueryCache
    {
        private readonly Dictionary<string, object> _cache = [];

        public IReadOnlyCollection<string> Keys => _cache.Keys.ToArray();

        public Task<TResponse?> GetAsync<TResponse>(string key, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(key, out var value) && value is TResponse typed)
            {
                return Task.FromResult<TResponse?>(typed);
            }

            return Task.FromResult<TResponse?>(default);
        }

        public Task SetAsync<TResponse>(string key, TResponse response, TimeSpan? ttl, CancellationToken cancellationToken = default)
        {
            _cache[key] = response!;
            return Task.CompletedTask;
        }
    }
}
