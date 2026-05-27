using ProductCatalog.Domain.Exceptions;

namespace ProductCatalog.Domain.ValueObjects;

public readonly record struct Money
{
    private Money(decimal value)
    {
        Value = value;
    }

    public decimal Value { get; }

    public static Money Create(decimal value)
    {
        if (value < 0)
        {
            throw new InvalidMoneyException($"El valor monetario '{value}' no puede ser negativo.");
        }

        if (GetScale(value) > 4)
        {
            throw new InvalidMoneyException($"El valor monetario '{value}' excede la escala 4.");
        }

        return new Money(value);
    }

    public static Money operator +(Money left, Money right)
    {
        return Create(left.Value + right.Value);
    }

    public static Money operator -(Money left, Money right)
    {
        return Create(left.Value - right.Value);
    }

    public static bool operator >(Money left, Money right)
    {
        return left.Value > right.Value;
    }

    public static bool operator <(Money left, Money right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >=(Money left, Money right)
    {
        return left.Value >= right.Value;
    }

    public static bool operator <=(Money left, Money right)
    {
        return left.Value <= right.Value;
    }

    public override string ToString()
    {
        return Value.ToString("0.####");
    }

    private static int GetScale(decimal value)
    {
        return (decimal.GetBits(value)[3] >> 16) & 0x7F;
    }
}
