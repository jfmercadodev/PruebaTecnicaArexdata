namespace ProductCatalog.Application.Products.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Name,
    string Sku,
    decimal SalePrice,
    decimal Cost,
    int Stock,
    decimal MarginPercent);
