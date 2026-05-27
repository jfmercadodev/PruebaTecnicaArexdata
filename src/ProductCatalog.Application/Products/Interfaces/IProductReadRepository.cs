using ProductCatalog.Application.Common.Enums;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Specifications;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Application.Products.Interfaces;

public interface IProductReadRepository
{
    Task<PagedResult<Product>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortField,
        SortDirection sortDirection,
        ISpecification<Product>? filter = null,
        CancellationToken cancellationToken = default);

    Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken cancellationToken = default);
}
