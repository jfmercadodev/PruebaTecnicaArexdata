using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Application.Products.Commands.CreateProduct;
using ProductCatalog.Application.Products.Mappings;
using ProductCatalog.Infrastructure.Repositories;

namespace ProductCatalog.IntegrationTests.Application;

public sealed class CreateProductCommandPersistenceTests
{
    [Fact]
    public async Task Handle_ShouldNotPublishEvent_WhenSaveChangesFails()
    {
        using var scope = ApplicationDbContextScope.Create();
        var domainEventPublisher = new RecordingDomainEventPublisher();
        var cacheInvalidationService = new RecordingCacheInvalidationService();

        var handler = new CreateProductCommandHandler(
            new ProductReadRepository(scope.DbContext),
            new ProductWriteRepository(scope.DbContext),
            new ThrowingUnitOfWork(),
            domainEventPublisher,
            new NoOpIdempotencyStore(),
            cacheInvalidationService,
            CreateMapper());

        var act = () => handler.Handle(
            new CreateProductCommand("Mechanical Keyboard", "KB-FAIL-001", 120m, 80m, 3),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Simulated persistence failure.");
        domainEventPublisher.PublishCount.Should().Be(0);
        cacheInvalidationService.InvalidationCount.Should().Be(0);
    }

    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(
            expression => expression.AddProfile<ProductProfile>(),
            NullLoggerFactory.Instance);

        return configuration.CreateMapper();
    }

    private sealed class ThrowingUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated persistence failure.");
        }
    }

    private sealed class RecordingDomainEventPublisher : IDomainEventPublisher
    {
        public int PublishCount { get; private set; }

        public Task PublishAsync(
            IReadOnlyCollection<ProductCatalog.Domain.Events.DomainEvent> domainEvents,
            CancellationToken cancellationToken = default)
        {
            PublishCount += domainEvents.Count;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingCacheInvalidationService : ICacheInvalidationService
    {
        public int InvalidationCount { get; private set; }

        public Task InvalidateByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
        {
            InvalidationCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpIdempotencyStore : IIdempotencyStore
    {
        public Task<IdempotencyRecord<TResponse>?> GetAsync<TResponse>(Guid requestId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IdempotencyRecord<TResponse>?>(null);
        }

        public Task SaveAsync<TResponse>(
            Guid requestId,
            TResponse response,
            TimeSpan window,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
