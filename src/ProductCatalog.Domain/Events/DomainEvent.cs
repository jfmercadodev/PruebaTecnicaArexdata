namespace ProductCatalog.Domain.Events;

public abstract record DomainEvent(DateTime OccurredOnUtc);
