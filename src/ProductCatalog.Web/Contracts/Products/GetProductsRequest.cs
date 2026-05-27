using ProductCatalog.Application.Common.Enums;

namespace ProductCatalog.Web.Contracts.Products;

public sealed record GetProductsRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SortField = "Name",
    SortDirection SortDirection = SortDirection.Asc,
    string? SearchTerm = null);
