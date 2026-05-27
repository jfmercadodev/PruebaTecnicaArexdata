using AutoMapper;
using MediatR;
using ProductCatalog.Application.Common.Exceptions;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Application.Products.Interfaces;

namespace ProductCatalog.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventPublisher _domainEventPublisher;
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly IMapper _mapper;

    public UpdateProductCommandHandler(
        IProductWriteRepository productWriteRepository,
        IUnitOfWork unitOfWork,
        IDomainEventPublisher domainEventPublisher,
        IIdempotencyStore idempotencyStore,
        ICacheInvalidationService cacheInvalidationService,
        IMapper mapper)
    {
        _productWriteRepository = productWriteRepository;
        _unitOfWork = unitOfWork;
        _domainEventPublisher = domainEventPublisher;
        _idempotencyStore = idempotencyStore;
        _cacheInvalidationService = cacheInvalidationService;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.RequestId.HasValue)
        {
            var existingResponse = await _idempotencyStore.GetAsync<ProductDto>(request.RequestId.Value, cancellationToken);
            if (existingResponse is not null)
            {
                return existingResponse.Response;
            }
        }

        var product = await _productWriteRepository.FindByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product '{request.ProductId}' was not found.");

        product.ExecuteAtomic(candidate =>
        {
            if (request.SalePrice.HasValue && request.Cost.HasValue)
            {
                candidate.UpdatePrice(request.SalePrice.Value, request.Cost.Value);
            }

            if (request.StockDelta.HasValue)
            {
                candidate.AdjustStock(request.StockDelta.Value);
            }
        });

        _productWriteRepository.Update(product);
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
