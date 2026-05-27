using ProductCatalog.Application.Common.Enums;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Application.Products.Interfaces;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Specifications;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.UnitTests.Application;

internal sealed class InMemoryProductReadRepository : IProductReadRepository
{
    private readonly List<Product> _products = [];

    public void Seed(params string[] skus)
    {
        foreach (var sku in skus)
        {
            _products.Add(Product.Create(
                "Seeded Product",
                Sku.Create(sku),
                Money.Create(120m),
                Money.Create(70m),
                10));
        }
    }

    public Task<bool> ExistsBySkuAsync(Sku sku, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_products.Any(product => product.Sku == sku));
    }

    public Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_products.SingleOrDefault(product => product.Id == id));
    }

    public Task<PagedResult<Product>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? sortField,
        SortDirection sortDirection,
        ISpecification<Product>? filter = null,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Product> query = _products;
        if (filter is not null)
        {
            query = query.Where(filter.ToExpression().Compile());
        }

        var items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToArray();
        return Task.FromResult(new PagedResult<Product>(items, pageNumber, pageSize, query.Count()));
    }
}
