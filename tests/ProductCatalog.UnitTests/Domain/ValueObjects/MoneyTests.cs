using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.UnitTests.Domain.ValueObjects;

public sealed class MoneyTests
{
    [Fact]
    public void Create_ShouldThrow_WhenValueIsNegative()
    {
        var action = () => Money.Create(-1m);

        action.Should().Throw<InvalidMoneyException>();
    }

    [Fact]
    public void Create_ShouldBeStructurallyEqual_WhenValuesMatch()
    {
        var left = Money.Create(10.5m);
        var right = Money.Create(10.5m);

        left.Should().Be(right);
    }

    [Fact]
    public void AddAndSubtract_ShouldReturnExpectedValues()
    {
        var left = Money.Create(120m);
        var right = Money.Create(70m);

        var sum = left + right;
        var difference = left - right;

        sum.Value.Should().Be(190m);
        difference.Value.Should().Be(50m);
    }

    [Fact]
    public void Subtract_ShouldThrow_WhenResultWouldBeNegative()
    {
        var left = Money.Create(10m);
        var right = Money.Create(11m);

        var action = () => _ = left - right;

        action.Should().Throw<InvalidMoneyException>();
    }
}
