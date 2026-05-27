using System.Linq.Expressions;

namespace ProductCatalog.Domain.Specifications;

public sealed class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _inner;

    public NotSpecification(Specification<T> inner)
    {
        _inner = inner;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        return ExpressionCombiner.Not(_inner.ToExpression());
    }
}
