using ProductCatalog.Application.Common.Models;

namespace ProductCatalog.Application.Common.Abstractions;

public interface IMetadataAwareResponse<out TResponse>
{
    TResponse WithMetadata(QueryMetadata metadata);
}
