using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Products.Interfaces;

public interface IProductWriteRepository
{
    Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(Product product);

    void Update(Product product);

    void Delete(Product product);
}
