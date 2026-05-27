using AutoMapper;
using MediatR;
using ProductCatalog.Application.Common.Exceptions;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Application.Products.Interfaces;

namespace ProductCatalog.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IMapper _mapper;

    public GetProductByIdQueryHandler(IProductReadRepository productReadRepository, IMapper mapper)
    {
        _productReadRepository = productReadRepository;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _productReadRepository.FindByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException($"No se encontro el producto '{request.ProductId}'.");

        return _mapper.Map<ProductDto>(product);
    }
}
