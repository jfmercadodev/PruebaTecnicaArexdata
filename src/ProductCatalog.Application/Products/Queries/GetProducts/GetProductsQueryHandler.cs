using AutoMapper;
using MediatR;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Application.Products.Interfaces;
using ProductCatalog.Application.Products.Specifications;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Specifications;

namespace ProductCatalog.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly IMapper _mapper;

    public GetProductsQueryHandler(IProductReadRepository productReadRepository, IMapper mapper)
    {
        _productReadRepository = productReadRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        ISpecification<Product>? specification = null;
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            specification = new ProductSearchSpecification(request.SearchTerm);
        }

        var result = await _productReadRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            request.SortField,
            request.SortDirection,
            specification,
            cancellationToken);

        return new PagedResult<ProductDto>(
            result.Items.Select(product => _mapper.Map<ProductDto>(product)).ToArray(),
            result.PageNumber,
            result.PageSize,
            result.TotalCount,
            result.Metadata);
    }
}
