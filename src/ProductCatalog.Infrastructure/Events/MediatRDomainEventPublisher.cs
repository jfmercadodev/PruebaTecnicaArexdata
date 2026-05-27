using MediatR;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Domain.Events;

namespace ProductCatalog.Infrastructure.Events;

public sealed class MediatRDomainEventPublisher : IDomainEventPublisher
{
    private readonly IPublisher _publisher;

    public MediatRDomainEventPublisher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task PublishAsync(IReadOnlyCollection<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = Activator.CreateInstance(notificationType, domainEvent)
                ?? throw new InvalidOperationException($"Could not create notification for {domainEvent.GetType().Name}.");

            await _publisher.Publish(notification, cancellationToken);
        }
    }
}
