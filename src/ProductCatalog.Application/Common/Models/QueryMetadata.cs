namespace ProductCatalog.Application.Common.Models;

public sealed record QueryMetadata(
    bool CacheHit,
    string Source,
    long ElapsedMs);
