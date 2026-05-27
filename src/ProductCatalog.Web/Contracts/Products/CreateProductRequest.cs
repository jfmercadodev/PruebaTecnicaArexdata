namespace ProductCatalog.Web.Contracts.Products;

public sealed record CreateProductRequest(
    string Name,
    string Sku,
    decimal SalePrice,
    decimal Cost,
    int Stock);
