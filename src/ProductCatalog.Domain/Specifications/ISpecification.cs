using System.Linq.Expressions;

namespace ProductCatalog.Domain.Specifications;

public interface ISpecification<T>
{
    Expression<Func<T, bool>> ToExpression();

    bool IsSatisfiedBy(T candidate);
}
