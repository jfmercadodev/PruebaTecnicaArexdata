using Microsoft.EntityFrameworkCore;
using ProductCatalog.Application.Products.Interfaces;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.Persistence;

namespace ProductCatalog.Infrastructure.Repositories;

public sealed class ProductWriteRepository : IProductWriteRepository
{
    private readonly AppDbContext _dbContext;

    public ProductWriteRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(Product product)
    {
        _dbContext.Products.Add(product);
    }

    public void Delete(Product product)
    {
        _dbContext.Attach(product);
        _dbContext.Entry(product).Property("IsDeleted").CurrentValue = true;
    }

    public async Task<Product?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
    }

    public void Update(Product product)
    {
        _dbContext.Products.Update(product);
    }
}
