namespace ProductCatalog.Application.Common.Abstractions;

public interface ICacheableQuery
{
    string CacheKey { get; }

    TimeSpan? Ttl { get; }
}
