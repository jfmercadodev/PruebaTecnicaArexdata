using MediatR;
using ProductCatalog.Application.Common.Abstractions;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Common.Models;

namespace ProductCatalog.Application.Common.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IQueryCache _queryCache;

    public CachingBehavior(IQueryCache queryCache)
    {
        _queryCache = queryCache;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheableQuery)
        {
            return await next();
        }

        var startedAt = DateTime.UtcNow;
        var cachedResponse = await _queryCache.GetAsync<TResponse>(cacheableQuery.CacheKey, cancellationToken);
        if (cachedResponse is not null)
        {
            var elapsedOnHit = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            return ApplyMetadata(cachedResponse, new QueryMetadata(true, "Cache", elapsedOnHit));
        }

        var response = await next();
        if (response is not IMetadataAwareResponse<TResponse> metadataAwareResponse)
        {
            return response;
        }

        var elapsedOnMiss = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
        var responseWithMetadata = metadataAwareResponse.WithMetadata(new QueryMetadata(false, "Base de datos", elapsedOnMiss));
        await _queryCache.SetAsync(cacheableQuery.CacheKey, responseWithMetadata, cacheableQuery.Ttl, cancellationToken);
        return responseWithMetadata;
    }

    private static TResponse ApplyMetadata(TResponse response, QueryMetadata metadata)
    {
        if (response is IMetadataAwareResponse<TResponse> metadataAwareResponse)
        {
            return metadataAwareResponse.WithMetadata(metadata);
        }

        return response;
    }
}
