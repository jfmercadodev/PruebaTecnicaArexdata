using ProductCatalog.Application.Common.Exceptions;
using ProductCatalog.Application.Products.Commands.CreateProduct;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Domain.Events;

namespace ProductCatalog.UnitTests.Application.Commands.CreateProduct;

public sealed class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateProduct_AndPublishCreatedEvent()
    {
        var readRepository = new InMemoryProductReadRepository();
        var writeRepository = new InMemoryProductWriteRepository();
        var unitOfWork = new FakeUnitOfWork();
        var eventPublisher = new FakeDomainEventPublisher();
        var idempotencyStore = new FakeIdempotencyStore();
        var cacheInvalidationService = new FakeCacheInvalidationService();
        var handler = new CreateProductCommandHandler(
            readRepository,
            writeRepository,
            unitOfWork,
            eventPublisher,
            idempotencyStore,
            cacheInvalidationService,
            MapperFactory.Create());

        var response = await handler.Handle(
            new CreateProductCommand("Mechanical Keyboard", "mkb--001", 120m, 70m, 15),
            CancellationToken.None);

        response.Sku.Should().Be("MKB-001");
        writeRepository.AddedProducts.Should().ContainSingle();
        unitOfWork.SaveCalls.Should().Be(1);
        cacheInvalidationService.InvalidatedPrefixes.Should().ContainSingle().Which.Should().Be("products:");
        eventPublisher.PublishedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductCreated>();
    }

    [Fact]
    public async Task Handle_ShouldThrowConflict_WhenSkuAlreadyExists()
    {
        var readRepository = new InMemoryProductReadRepository();
        readRepository.Seed("MKB-001");
        var handler = new CreateProductCommandHandler(
            readRepository,
            new InMemoryProductWriteRepository(),
            new FakeUnitOfWork(),
            new FakeDomainEventPublisher(),
            new FakeIdempotencyStore(),
            new FakeCacheInvalidationService(),
            MapperFactory.Create());

        var action = async () => await handler.Handle(
            new CreateProductCommand("Mechanical Keyboard", "mkb-001", 120m, 70m, 15),
            CancellationToken.None);

        await action.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedResponse_WhenRequestIdAlreadyExists()
    {
        var cachedResponse = new ProductDto(
            Guid.NewGuid(),
            "Mechanical Keyboard",
            "MKB-001",
            120m,
            70m,
            15,
            41.6667m);

        var idempotencyStore = new FakeIdempotencyStore();
        var requestId = Guid.NewGuid();
        await idempotencyStore.SaveAsync(requestId, cachedResponse, TimeSpan.FromMinutes(10), CancellationToken.None);

        var writeRepository = new InMemoryProductWriteRepository();
        var handler = new CreateProductCommandHandler(
            new InMemoryProductReadRepository(),
            writeRepository,
            new FakeUnitOfWork(),
            new FakeDomainEventPublisher(),
            idempotencyStore,
            new FakeCacheInvalidationService(),
            MapperFactory.Create());

        var response = await handler.Handle(
            new CreateProductCommand("Mechanical Keyboard", "mkb-001", 120m, 70m, 15, requestId),
            CancellationToken.None);

        response.Should().Be(cachedResponse);
        writeRepository.AddedProducts.Should().BeEmpty();
    }
}
