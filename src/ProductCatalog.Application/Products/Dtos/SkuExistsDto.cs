namespace ProductCatalog.Application.Products.Dtos;

public sealed record SkuExistsDto(
    string Sku,
    bool Exists);
