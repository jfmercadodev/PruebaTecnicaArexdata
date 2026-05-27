using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.UnitTests.Domain.ValueObjects;

public sealed class SkuTests
{
    [Fact]
    public void Create_ShouldNormalizeTrimUppercaseAndHyphens()
    {
        var sku = Sku.Create("  mkb--001  ");

        sku.Value.Should().Be("MKB-001");
    }

    [Theory]
    [InlineData("ab0-cd", SkuZeroSubstitution.ZeroToLetterO, "ABO-CD")]
    [InlineData("aob-cdo", SkuZeroSubstitution.LetterOToZero, "A0B-CD0")]
    public void Create_ShouldApplyConfiguredZeroSubstitution(
        string rawValue,
        SkuZeroSubstitution substitution,
        string expected)
    {
        var sku = Sku.Create(rawValue, substitution);

        sku.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_ShouldThrow_WhenSkuContainsInvalidCharacters()
    {
        var action = () => Sku.Create("abc_001");

        action.Should().Throw<InvalidSkuException>();
    }

    [Fact]
    public void Create_ShouldBeStructurallyEqual_WhenValuesMatch()
    {
        var left = Sku.Create("mkb--001");
        var right = Sku.Create("MKB-001");

        left.Should().Be(right);
    }
}
