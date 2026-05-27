namespace ProductCatalog.Infrastructure.Persistence.Entities;

public sealed class ProcessedRequestEntity
{
    public Guid RequestId { get; set; }

    public string ResponseType { get; set; } = string.Empty;

    public string PayloadJson { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime ExpiresAtUtc { get; set; }
}
