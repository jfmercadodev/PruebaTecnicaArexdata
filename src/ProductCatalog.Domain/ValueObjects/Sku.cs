using System.Text.RegularExpressions;
using ProductCatalog.Domain.Exceptions;

namespace ProductCatalog.Domain.ValueObjects;

public sealed partial record Sku
{
    private Sku(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Sku Create(string? value, SkuZeroSubstitution substitution = SkuZeroSubstitution.None)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidSkuException("SKU cannot be empty.");
        }

        var normalized = value.Trim().ToUpperInvariant();
        normalized = WhitespaceRegex().Replace(normalized, " ");
        normalized = HyphenRegex().Replace(normalized, "-");
        normalized = substitution switch
        {
            SkuZeroSubstitution.ZeroToLetterO => normalized.Replace('0', 'O'),
            SkuZeroSubstitution.LetterOToZero => normalized.Replace('O', '0'),
            _ => normalized
        };

        if (normalized.Length is < 3 or > 50)
        {
            throw new InvalidSkuException("SKU length must be between 3 and 50 characters.");
        }

        if (!AllowedCharactersRegex().IsMatch(normalized))
        {
            throw new InvalidSkuException("SKU only allows alphanumeric characters and hyphens.");
        }

        return new Sku(normalized);
    }

    public override string ToString()
    {
        return Value;
    }

    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex("-{2,}", RegexOptions.Compiled)]
    private static partial Regex HyphenRegex();

    [GeneratedRegex("^[A-Z0-9-]+$", RegexOptions.Compiled)]
    private static partial Regex AllowedCharactersRegex();
}
