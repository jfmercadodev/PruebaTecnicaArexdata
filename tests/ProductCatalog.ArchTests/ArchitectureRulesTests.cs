using System.Reflection;
using System.Runtime.CompilerServices;
using NetArchTest.Rules;
using ProductCatalog.Application.Products.Commands.CreateProduct;
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.ArchTests;

public sealed class ArchitectureRulesTests
{
    private static readonly Assembly DomainAssembly = typeof(Money).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(CreateProductCommand).Assembly;

    [Fact]
    public void Domain_Should_Not_Depend_On_Application_Or_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny("ProductCatalog.Application", "ProductCatalog.Infrastructure")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Handlers_Should_Not_Depend_On_EfCore_Or_Infrastructure_DbContext()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.EntityFrameworkCore", "ProductCatalog.Infrastructure.Persistence")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void ValueObjects_Should_Not_Expose_Public_Mutable_Setters()
    {
        var valueObjectTypes = DomainAssembly.GetTypes()
            .Where(type => type.Namespace == "ProductCatalog.Domain.ValueObjects")
            .Where(type => !type.IsEnum)
            .Where(type => type.IsClass || type.IsValueType)
            .ToArray();

        Assert.NotEmpty(valueObjectTypes);

        var offendingProperties = valueObjectTypes
            .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(HasPublicMutableSetter)
                .Select(property => $"{type.FullName}.{property.Name}"))
            .ToArray();

        Assert.True(offendingProperties.Length == 0, string.Join(", ", offendingProperties));
    }

    [Fact]
    public void DomainEvents_Should_Be_Immutable()
    {
        var domainEventTypes = DomainAssembly.GetTypes()
            .Where(type => typeof(DomainEvent).IsAssignableFrom(type))
            .Where(type => !type.IsAbstract)
            .ToArray();

        Assert.NotEmpty(domainEventTypes);

        var offendingProperties = domainEventTypes
            .SelectMany(type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(HasPublicMutableSetter)
                .Select(property => $"{type.FullName}.{property.Name}"))
            .ToArray();

        Assert.True(offendingProperties.Length == 0, string.Join(", ", offendingProperties));
    }

    private static bool HasPublicMutableSetter(PropertyInfo propertyInfo)
    {
        var setMethod = propertyInfo.SetMethod;
        if (setMethod is null || !setMethod.IsPublic)
        {
            return false;
        }

        var requiredModifiers = setMethod.ReturnParameter.GetRequiredCustomModifiers();
        return !requiredModifiers.Contains(typeof(IsExternalInit));
    }
}
