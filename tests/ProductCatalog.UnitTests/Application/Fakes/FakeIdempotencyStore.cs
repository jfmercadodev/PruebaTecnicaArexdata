using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Common.Models;

namespace ProductCatalog.UnitTests.Application;

internal sealed class FakeIdempotencyStore : IIdempotencyStore
{
    private readonly Dictionary<Guid, object> _responses = [];

    public Task<IdempotencyRecord<TResponse>?> GetAsync<TResponse>(Guid requestId, CancellationToken cancellationToken = default)
    {
        if (_responses.TryGetValue(requestId, out var response) && response is TResponse typedResponse)
        {
            return Task.FromResult<IdempotencyRecord<TResponse>?>(new IdempotencyRecord<TResponse>(requestId, typedResponse));
        }

        return Task.FromResult<IdempotencyRecord<TResponse>?>(null);
    }

    public Task SaveAsync<TResponse>(Guid requestId, TResponse response, TimeSpan window, CancellationToken cancellationToken = default)
    {
        _responses[requestId] = response!;
        return Task.CompletedTask;
    }
}
