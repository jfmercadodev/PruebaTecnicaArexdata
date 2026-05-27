using ProductCatalog.Domain.Events;

namespace ProductCatalog.Domain.Common;

public abstract class AggregateRoot<TKey> : Entity<TKey> where TKey : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected AggregateRoot(TKey id)
        : base(id)
    {
    }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    protected void ReplaceDomainEvents(IEnumerable<DomainEvent> domainEvents)
    {
        _domainEvents.Clear();
        _domainEvents.AddRange(domainEvents);
    }
}
