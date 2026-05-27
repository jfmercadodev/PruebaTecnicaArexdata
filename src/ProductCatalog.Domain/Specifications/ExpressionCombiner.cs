using System.Linq.Expressions;

namespace ProductCatalog.Domain.Specifications;

internal static class ExpressionCombiner
{
    public static Expression<Func<T, bool>> And<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        return Combine(left, right, Expression.AndAlso);
    }

    public static Expression<Func<T, bool>> Or<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        return Combine(left, right, Expression.OrElse);
    }

    public static Expression<Func<T, bool>> Not<T>(Expression<Func<T, bool>> expression)
    {
        var parameter = expression.Parameters[0];
        var body = Expression.Not(expression.Body);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression<Func<T, bool>> Combine<T>(
        Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right,
        Func<Expression, Expression, BinaryExpression> combiner)
    {
        var parameter = Expression.Parameter(typeof(T), "candidate");
        var leftBody = ReplaceParameter(left, parameter);
        var rightBody = ReplaceParameter(right, parameter);
        return Expression.Lambda<Func<T, bool>>(combiner(leftBody, rightBody), parameter);
    }

    private static Expression ReplaceParameter<T>(
        Expression<Func<T, bool>> expression,
        ParameterExpression parameter)
    {
        return new ParameterReplaceVisitor(expression.Parameters[0], parameter).Visit(expression.Body)!;
    }

    private sealed class ParameterReplaceVisitor(
        ParameterExpression source,
        ParameterExpression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source ? target : base.VisitParameter(node);
        }
    }
}
