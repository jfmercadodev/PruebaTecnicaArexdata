namespace ProductCatalog.Application.Common.Models;

public sealed record IdempotencyRecord<TResponse>(
    Guid RequestId,
    TResponse Response);
