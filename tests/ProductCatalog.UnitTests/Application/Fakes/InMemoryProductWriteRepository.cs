using ProductCatalog.Application.Products.Interfaces;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.UnitTests.Application;

internal sealed class InMemoryProductWriteRepository : IProductWriteRepository
{
    private readonly Dictionary<Guid, Product> _products = [];

    public InMemoryProductWriteRepository(params Product[] seededProducts)
    {
        foreach (var product in seededProducts)
        {
            _products[product.Id] = product;
        }
    }

    public List<Product> AddedProducts { get; } = [];

    public List<Product> UpdatedProducts { get; } = [];

    public List<Product> DeletedProducts { get; } = [];

    public void Add(Product product)
    {
        AddedProducts.Add(product);
        _products[product.Id] = product;
    }

    public void Delete(Product product)
    {
        DeletedProducts.Add(product);
        _products.Remove(product.Id);
    }

    public Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _products.TryGetValue(id, out var product);
        return Task.FromResult(product);
    }

    public void Update(Product product)
    {
        UpdatedProducts.Add(product);
        _products[product.Id] = product;
    }
}
