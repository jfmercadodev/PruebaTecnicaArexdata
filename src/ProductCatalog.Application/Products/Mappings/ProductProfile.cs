using AutoMapper;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Products.Mappings;

public sealed class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>(MemberList.Destination)
            .ForCtorParam(nameof(ProductDto.Sku), opt => opt.MapFrom(product => product.Sku.Value))
            .ForCtorParam(nameof(ProductDto.SalePrice), opt => opt.MapFrom(product => product.SalePrice.Value))
            .ForCtorParam(nameof(ProductDto.Cost), opt => opt.MapFrom(product => product.Cost.Value))
            .ForCtorParam(nameof(ProductDto.MarginPercent), opt => opt.MapFrom(product => product.MarginPercent));
    }
}
