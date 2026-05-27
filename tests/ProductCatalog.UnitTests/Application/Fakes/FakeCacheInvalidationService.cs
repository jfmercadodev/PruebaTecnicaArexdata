using ProductCatalog.Application.Common.Interfaces;

namespace ProductCatalog.UnitTests.Application;

internal sealed class FakeCacheInvalidationService : ICacheInvalidationService
{
    public List<string> InvalidatedPrefixes { get; } = [];

    public Task InvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        InvalidatedPrefixes.Add(prefix);
        return Task.CompletedTask;
    }
}
