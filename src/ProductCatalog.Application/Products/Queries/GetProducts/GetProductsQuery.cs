using ProductCatalog.Application.Common.Abstractions;
using ProductCatalog.Application.Common.Enums;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery(
    int PageNumber,
    int PageSize,
    string? SortField,
    SortDirection SortDirection,
    string? SearchTerm = null) : IQuery<PagedResult<ProductDto>>, ICacheableQuery
{
    public string CacheKey =>
        $"products:p{PageNumber}:s{PageSize}:{(SortField ?? "name").ToLowerInvariant()}:{SortDirection.ToString().ToLowerInvariant()}:{(SearchTerm ?? "all").Trim().ToLowerInvariant()}";

    public TimeSpan? Ttl => TimeSpan.FromMinutes(5);
}
