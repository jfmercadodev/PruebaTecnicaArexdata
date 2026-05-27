using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Domain.Events;

namespace ProductCatalog.UnitTests.Application;

internal sealed class FakeDomainEventPublisher : IDomainEventPublisher
{
    public List<DomainEvent> PublishedEvents { get; } = [];

    public Task PublishAsync(IReadOnlyCollection<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        PublishedEvents.AddRange(domainEvents);
        return Task.CompletedTask;
    }
}
