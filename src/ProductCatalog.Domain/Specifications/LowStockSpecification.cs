using System.Linq.Expressions;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Specifications;

public sealed class LowStockSpecification : Specification<Product>
{
    private readonly int _threshold;

    public LowStockSpecification(int threshold)
    {
        _threshold = threshold;
    }

    public override Expression<Func<Product, bool>> ToExpression()
    {
        return product => product.Stock < _threshold;
    }
}
