using ProductCatalog.Application.Products.Commands.UpdateProduct;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Events;
using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.UnitTests.Application.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateProduct_AndPublishSingleUpdatedEvent()
    {
        var product = CreatePersistedProduct();
        var writeRepository = new InMemoryProductWriteRepository(product);
        var unitOfWork = new FakeUnitOfWork();
        var eventPublisher = new FakeDomainEventPublisher();
        var cacheInvalidationService = new FakeCacheInvalidationService();
        var handler = new UpdateProductCommandHandler(
            writeRepository,
            unitOfWork,
            eventPublisher,
            new FakeIdempotencyStore(),
            cacheInvalidationService,
            MapperFactory.Create());

        var response = await handler.Handle(
            new UpdateProductCommand(product.Id, 135m, 80m, -2),
            CancellationToken.None);

        response.SalePrice.Should().Be(135m);
        response.Cost.Should().Be(80m);
        response.Stock.Should().Be(13);
        cacheInvalidationService.InvalidatedPrefixes.Should().ContainSingle().Which.Should().Be("products:");
        eventPublisher.PublishedEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProductUpdated>();
    }

    [Fact]
    public async Task Handle_ShouldRollback_WhenCompositeUpdateFails()
    {
        var product = CreatePersistedProduct();
        var writeRepository = new InMemoryProductWriteRepository(product);
        var unitOfWork = new FakeUnitOfWork();
        var eventPublisher = new FakeDomainEventPublisher();
        var cacheInvalidationService = new FakeCacheInvalidationService();
        var handler = new UpdateProductCommandHandler(
            writeRepository,
            unitOfWork,
            eventPublisher,
            new FakeIdempotencyStore(),
            cacheInvalidationService,
            MapperFactory.Create());

        var action = async () => await handler.Handle(
            new UpdateProductCommand(product.Id, 60m, 80m, -4),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidPriceException>();
        product.SalePrice.Value.Should().Be(120m);
        product.Cost.Value.Should().Be(70m);
        product.Stock.Should().Be(15);
        cacheInvalidationService.InvalidatedPrefixes.Should().BeEmpty();
        unitOfWork.SaveCalls.Should().Be(0);
        eventPublisher.PublishedEvents.Should().BeEmpty();
    }

    private static Product CreatePersistedProduct()
    {
        var product = Product.Create(
            "Mechanical Keyboard",
            Sku.Create("MKB-001"),
            Money.Create(120m),
            Money.Create(70m),
            15);

        product.ClearDomainEvents();
        return product;
    }
}
