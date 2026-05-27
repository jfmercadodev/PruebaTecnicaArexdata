namespace ProductCatalog.Web.Contracts.Products;

public sealed record UpdateProductRequest(
    string? Name,
    string? Sku,
    decimal? SalePrice,
    decimal? Cost,
    int? StockDelta);
