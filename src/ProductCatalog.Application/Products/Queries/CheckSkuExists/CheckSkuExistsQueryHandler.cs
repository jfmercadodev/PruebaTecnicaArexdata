using MediatR;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Application.Products.Interfaces;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Application.Products.Queries.CheckSkuExists;

public sealed class CheckSkuExistsQueryHandler : IRequestHandler<CheckSkuExistsQuery, SkuExistsDto>
{
    private readonly IProductReadRepository _productReadRepository;

    public CheckSkuExistsQueryHandler(IProductReadRepository productReadRepository)
    {
        _productReadRepository = productReadRepository;
    }

    public async Task<SkuExistsDto> Handle(CheckSkuExistsQuery request, CancellationToken cancellationToken)
    {
        var normalizedSku = Sku.Create(request.Sku);
        var exists = await _productReadRepository.ExistsBySkuAsync(normalizedSku, cancellationToken);
        return new SkuExistsDto(normalizedSku.Value, exists);
    }
}
