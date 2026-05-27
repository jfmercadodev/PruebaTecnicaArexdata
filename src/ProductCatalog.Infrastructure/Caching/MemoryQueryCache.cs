using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using ProductCatalog.Application.Common.Interfaces;

namespace ProductCatalog.Infrastructure.Caching;

public sealed class MemoryQueryCache : IQueryCache, ICacheInvalidationService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, byte> _keys = new(StringComparer.OrdinalIgnoreCase);

    public MemoryQueryCache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<TResponse?> GetAsync<TResponse>(string key, CancellationToken cancellationToken = default)
    {
        var value = _memoryCache.TryGetValue(key, out var cached)
            ? (TResponse?)cached
            : default;

        return Task.FromResult(value);
    }

    public Task InvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var keysToInvalidate = _keys.Keys
            .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        foreach (var key in keysToInvalidate)
        {
            _memoryCache.Remove(key);
            _keys.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public Task SetAsync<TResponse>(
        string key,
        TResponse response,
        TimeSpan? ttl,
        CancellationToken cancellationToken = default)
    {
        var options = new MemoryCacheEntryOptions();
        if (ttl.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = ttl;
        }

        options.RegisterPostEvictionCallback((cacheKey, _, _, state) =>
        {
            if (cacheKey is string stringKey && state is ConcurrentDictionary<string, byte> dictionary)
            {
                dictionary.TryRemove(stringKey, out _);
            }
        }, _keys);

        _memoryCache.Set(key, response, options);
        _keys[key] = 0;

        return Task.CompletedTask;
    }
}
