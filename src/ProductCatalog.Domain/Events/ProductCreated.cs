namespace ProductCatalog.Domain.Events;

public sealed record ProductCreated(
    Guid ProductId,
    string Sku,
    string Name,
    decimal SalePrice,
    decimal Cost,
    int Stock,
    DateTime OccurredOnUtc) : DomainEvent(OccurredOnUtc);
