using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductCatalog.Infrastructure.Persistence.Entities;

namespace ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProcessedRequestEntityConfiguration : IEntityTypeConfiguration<ProcessedRequestEntity>
{
    public void Configure(EntityTypeBuilder<ProcessedRequestEntity> builder)
    {
        builder.ToTable("ProcessedRequests");

        builder.HasKey(entity => entity.RequestId);

        builder.Property(entity => entity.ResponseType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(entity => entity.PayloadJson)
            .IsRequired();

        builder.Property(entity => entity.CreatedAtUtc)
            .IsRequired();

        builder.Property(entity => entity.ExpiresAtUtc)
            .IsRequired();
    }
}
