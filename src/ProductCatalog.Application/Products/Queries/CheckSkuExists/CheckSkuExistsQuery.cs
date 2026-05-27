using ProductCatalog.Application.Common.Abstractions;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Application.Products.Queries.CheckSkuExists;

public sealed record CheckSkuExistsQuery(string Sku) : IQuery<SkuExistsDto>;
