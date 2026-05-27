using ProductCatalog.Application.Common.Abstractions;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string Sku,
    decimal SalePrice,
    decimal Cost,
    int Stock,
    Guid? RequestId = null) : IIdempotentCommand<ProductDto>
{
    public TimeSpan IdempotencyWindow { get; init; } = TimeSpan.FromMinutes(10);
}
