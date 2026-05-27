using MediatR;
using ProductCatalog.Application.Common.Exceptions;
using ProductCatalog.Application.Common.Interfaces;
using ProductCatalog.Application.Products.Interfaces;

namespace ProductCatalog.Application.Products.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IProductWriteRepository _productWriteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public DeleteProductCommandHandler(
        IProductWriteRepository productWriteRepository,
        IUnitOfWork unitOfWork,
        IIdempotencyStore idempotencyStore,
        ICacheInvalidationService cacheInvalidationService)
    {
        _productWriteRepository = productWriteRepository;
        _unitOfWork = unitOfWork;
        _idempotencyStore = idempotencyStore;
        _cacheInvalidationService = cacheInvalidationService;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        if (request.RequestId.HasValue)
        {
            var existingResponse = await _idempotencyStore.GetAsync<Unit>(request.RequestId.Value, cancellationToken);
            if (existingResponse is not null)
            {
                return existingResponse.Response;
            }
        }

        var product = await _productWriteRepository.FindByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException($"No se encontro el producto '{request.ProductId}'.");

        _productWriteRepository.Delete(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _cacheInvalidationService.InvalidateByPrefixAsync("products:", cancellationToken);

        if (request.RequestId.HasValue)
        {
            await _idempotencyStore.SaveAsync(
                request.RequestId.Value,
                Unit.Value,
                request.IdempotencyWindow,
                cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
