namespace ProductCatalog.Application.Common.Interfaces;

public interface IQueryCache
{
    Task<TResponse?> GetAsync<TResponse>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<TResponse>(
        string key,
        TResponse response,
        TimeSpan? ttl,
        CancellationToken cancellationToken = default);
}
