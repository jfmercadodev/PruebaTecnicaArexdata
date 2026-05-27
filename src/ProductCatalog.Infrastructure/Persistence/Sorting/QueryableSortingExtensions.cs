using System.Linq.Expressions;
using System.Reflection;
using ProductCatalog.Application.Common.Enums;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Persistence.Sorting;

internal static class QueryableSortingExtensions
{
    public static IQueryable<Product> OrderByProperty(
        this IQueryable<Product> query,
        string? sortField,
        SortDirection sortDirection)
    {
        var property = typeof(Product)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .SingleOrDefault(candidate => string.Equals(candidate.Name, sortField, StringComparison.OrdinalIgnoreCase))
            ?? typeof(Product).GetProperty(nameof(Product.Name))!;

        var parameter = Expression.Parameter(typeof(Product), "product");
        var body = Expression.Property(parameter, property);
        var lambda = Expression.Lambda(body, parameter);

        var methodName = sortDirection == SortDirection.Asc ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);
        var method = typeof(Queryable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(candidate =>
                candidate.Name == methodName &&
                candidate.IsGenericMethodDefinition &&
                candidate.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(Product), property.PropertyType);

        return (IQueryable<Product>)method.Invoke(null, [query, lambda])!;
    }
}
