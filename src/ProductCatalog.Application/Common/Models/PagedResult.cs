using ProductCatalog.Application.Common.Abstractions;

namespace ProductCatalog.Application.Common.Models;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    QueryMetadata? Metadata = null) : IMetadataAwareResponse<PagedResult<T>>
{
    public PagedResult<T> WithMetadata(QueryMetadata metadata)
    {
        return this with { Metadata = metadata };
    }
}
