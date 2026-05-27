using MediatR;
using ProductCatalog.Domain.Events;

namespace ProductCatalog.Infrastructure.Events;

public sealed record DomainEventNotification<TDomainEvent>(TDomainEvent DomainEvent) : INotification
    where TDomainEvent : DomainEvent;
