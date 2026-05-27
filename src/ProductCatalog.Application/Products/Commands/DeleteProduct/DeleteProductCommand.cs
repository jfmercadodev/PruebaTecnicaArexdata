using MediatR;
using ProductCatalog.Application.Common.Abstractions;

namespace ProductCatalog.Application.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(
    Guid ProductId,
    Guid? RequestId = null) : IIdempotentCommand<Unit>
{
    public TimeSpan IdempotencyWindow { get; init; } = TimeSpan.FromMinutes(10);
}
