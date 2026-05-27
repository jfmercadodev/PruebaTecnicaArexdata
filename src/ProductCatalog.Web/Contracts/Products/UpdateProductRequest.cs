namespace ProductCatalog.Web.Contracts.Products;

public sealed record UpdateProductRequest(
    decimal? SalePrice,
    decimal? Cost,
    int? StockDelta);
