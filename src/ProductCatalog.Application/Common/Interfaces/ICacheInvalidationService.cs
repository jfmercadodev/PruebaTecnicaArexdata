namespace ProductCatalog.Application.Common.Interfaces;

public interface ICacheInvalidationService
{
    Task InvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
