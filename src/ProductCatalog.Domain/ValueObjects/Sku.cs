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
            throw new InvalidSkuException("El SKU no puede estar vacio.");
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
            throw new InvalidSkuException("El SKU debe tener entre 3 y 50 caracteres.");
        }

        if (!AllowedCharactersRegex().IsMatch(normalized))
        {
            throw new InvalidSkuException("El SKU solo permite caracteres alfanumericos y guiones.");
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
