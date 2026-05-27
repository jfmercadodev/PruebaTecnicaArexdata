using ProductCatalog.Application.Common.Models;

namespace ProductCatalog.Application.Common.Interfaces;

public interface IIdempotencyStore
{
    Task<IdempotencyRecord<TResponse>?> GetAsync<TResponse>(Guid requestId, CancellationToken cancellationToken = default);

    Task SaveAsync<TResponse>(
        Guid requestId,
        TResponse response,
        TimeSpan window,
        CancellationToken cancellationToken = default);
}
