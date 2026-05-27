using System.Linq.Expressions;

namespace ProductCatalog.Domain.Specifications;

public abstract class Specification<T> : ISpecification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T candidate)
    {
        return ToExpression().Compile()(candidate);
    }

    public static Specification<T> operator &(Specification<T> left, Specification<T> right)
    {
        return new AndSpecification<T>(left, right);
    }

    public static Specification<T> operator |(Specification<T> left, Specification<T> right)
    {
        return new OrSpecification<T>(left, right);
    }

    public static Specification<T> operator !(Specification<T> specification)
    {
        return new NotSpecification<T>(specification);
    }

    public static bool operator true(Specification<T> specification)
    {
        return false;
    }

    public static bool operator false(Specification<T> specification)
    {
        return false;
    }
}
