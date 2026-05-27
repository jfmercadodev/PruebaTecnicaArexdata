using System.Linq.Expressions;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Specifications;

namespace ProductCatalog.Application.Products.Specifications;

public sealed class ProductSearchSpecification : Specification<Product>
{
    private readonly string _term;

    public ProductSearchSpecification(string term)
    {
        _term = term.Trim().ToUpperInvariant();
    }

    public string Term => _term;

    public override Expression<Func<Product, bool>> ToExpression()
    {
        return product =>
            product.Name.ToUpper().Contains(_term) ||
            product.Sku.Value.Contains(_term);
    }
}
