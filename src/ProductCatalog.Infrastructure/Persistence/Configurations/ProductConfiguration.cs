using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.Persistence.Converters;

namespace ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(product => product.Sku)
            .HasConversion<SkuValueConverter>()
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(product => product.Sku)
            .IsUnique();

        builder.Property(product => product.SalePrice)
            .HasConversion<MoneyValueConverter>()
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(product => product.Cost)
            .HasConversion<MoneyValueConverter>()
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(product => product.Stock)
            .IsRequired();

        builder.Property<bool>("IsDeleted")
            .HasDefaultValue(false);

        builder.HasQueryFilter(product => !EF.Property<bool>(product, "IsDeleted"));

        builder.Ignore(product => product.DomainEvents);
    }
}
