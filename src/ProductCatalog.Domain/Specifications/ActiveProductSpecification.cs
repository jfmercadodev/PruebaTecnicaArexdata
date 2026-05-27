using System.Linq.Expressions;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Specifications;

public sealed class ActiveProductSpecification : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression()
    {
        return product => product.Stock > 0;
    }
}
