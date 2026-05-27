using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Infrastructure.Persistence.Converters;

public sealed class SkuValueConverter : ValueConverter<Sku, string>
{
    public SkuValueConverter()
        : base(
            sku => sku.Value,
            value => Sku.Create(value, SkuZeroSubstitution.None))
    {
    }
}
