using AutoMapper;
using MediatR;
using ProductCatalog.Application.Common.Exceptions;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Application.Products.Interfaces;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(
        IProductReadRepository productReadRepository,
        IProductWriteRepository productWriteRepository,
        IUnitOfWork unitOfWork,
        IDomainEventPublisher domainEventPublisher,
        IIdempotencyStore idempotencyStore,
        ICacheInvalidationService cacheInvalidationService,
        IMapper mapper)
    {
        _productReadRepository = productReadRepository;
        _productWriteRepository = productWriteRepository;
        _unitOfWork = unitOfWork;
        _domainEventPublisher = domainEventPublisher;
        _idempotencyStore = idempotencyStore;
        _cacheInvalidationService = cacheInvalidationService;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.RequestId.HasValue)
        {
            var existingResponse = await _idempotencyStore.GetAsync<ProductDto>(request.RequestId.Value, cancellationToken);
            if (existingResponse is not null)
            {
                return existingResponse.Response;
            }
        }

        var sku = Sku.Create(request.Sku);
        if (await _productReadRepository.ExistsBySkuAsync(sku, cancellationToken))
        {
            throw new ConflictException($"Ya existe un producto con el SKU '{sku.Value}'.");
        }

        var product = Product.Create(
            request.Name,
            sku,
            Money.Create(request.SalePrice),
            Money.Create(request.Cost),
            request.Stock);

        _productWriteRepository.Add(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _domainEventPublisher.PublishAsync(product.DomainEvents, cancellationToken);
        await _cacheInvalidationService.InvalidateByPrefixAsync("products:", cancellationToken);

        var response = _mapper.Map<ProductDto>(product);

        if (request.RequestId.HasValue)
        {
            await _idempotencyStore.SaveAsync(
                request.RequestId.Value,
                response,
                request.IdempotencyWindow,
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
