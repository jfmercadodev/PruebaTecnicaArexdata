using ProductCatalog.Application.Common.Abstractions;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<ProductDto>;
