using ProductCatalog.Application.Common.Abstractions;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    decimal? SalePrice,
    decimal? Cost,
    int? StockDelta,
    Guid? RequestId = null) : IIdempotentCommand<ProductDto>
{
    public TimeSpan IdempotencyWindow { get; init; } = TimeSpan.FromMinutes(10);
}
