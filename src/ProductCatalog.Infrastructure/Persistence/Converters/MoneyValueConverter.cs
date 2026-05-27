using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Infrastructure.Persistence.Converters;

public sealed class MoneyValueConverter : ValueConverter<Money, decimal>
{
    public MoneyValueConverter()
        : base(
            money => money.Value,
            value => Money.Create(value))
    {
    }
}
