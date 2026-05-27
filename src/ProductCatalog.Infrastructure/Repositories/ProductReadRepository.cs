using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Common.Enums;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Application.Products.Interfaces;
using ProductCatalog.Application.Products.Specifications;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Specifications;
using ProductCatalog.Domain.ValueObjects;
using ProductCatalog.Infrastructure.Persistence;
using ProductCatalog.Infrastructure.Persistence.Sorting;

namespace ProductCatalog.Infrastructure.Repositories;

public sealed class ProductReadRepository : IProductReadRepository
{
    private readonly AppDbContext _dbContext;

    public ProductReadRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(product => product.Sku == sku, cancellationToken);
    }

    public async Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Product>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortField,
        SortDirection sortDirection,
        ISpecification<Product>? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (filter is ProductSearchSpecification searchSpecification)
        {
            var orderedItems = await _dbContext.Products
                .AsNoTracking()
                .OrderByProperty(sortField, sortDirection)
                .ToListAsync(cancellationToken);

            var filteredItems = orderedItems
                .Where(product =>
                    product.Name.Contains(searchSpecification.Term, StringComparison.OrdinalIgnoreCase) ||
                    product.Sku.Value.Contains(searchSpecification.Term, StringComparison.Ordinal))
                .ToArray();

            var pagedItems = filteredItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToArray();

            return new PagedResult<Product>(pagedItems, pageNumber, pageSize, filteredItems.Length);
        }

        IQueryable<Product> query = _dbContext.Products.AsNoTracking();

        if (filter is not null)
        {
            query = query.Where(filter.ToExpression());
        }

        query = query.OrderByProperty(sortField, sortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToArrayAsync(cancellationToken);

        return new PagedResult<Product>(items, pageNumber, pageSize, totalCount);
    }
}
