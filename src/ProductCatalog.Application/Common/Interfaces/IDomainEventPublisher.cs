using ProductCatalog.Domain.Events;

namespace ProductCatalog.Application.Common.Interfaces;

public interface IDomainEventPublisher
{
    Task PublishAsync(IReadOnlyCollection<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
