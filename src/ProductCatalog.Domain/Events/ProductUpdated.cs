namespace ProductCatalog.Domain.Events;

public sealed record ProductUpdated(
    Guid ProductId,
    string Sku,
    decimal SalePrice,
    decimal Cost,
    int Stock,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
