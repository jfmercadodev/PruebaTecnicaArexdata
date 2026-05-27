using System.Diagnostics;
using MediatR;
using ProductCatalog.Application.Common.Enums;
using ProductCatalog.Application.Common.Models;
using ProductCatalog.Application.Products.Commands.CreateProduct;
using ProductCatalog.Application.Products.Commands.DeleteProduct;
using ProductCatalog.Application.Products.Commands.UpdateProduct;
using ProductCatalog.Application.Products.Dtos;
using ProductCatalog.Application.Products.Queries.CheckSkuExists;
using ProductCatalog.Application.Products.Queries.GetProductById;
using ProductCatalog.Application.Products.Queries.GetProducts;
using ProductCatalog.Web.Models;

namespace ProductCatalog.Web.Services;

public sealed class ProductCatalogFacade
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CatalogProblemDetailsFactory _problemDetailsFactory;
    private readonly ISender _sender;

    public ProductCatalogFacade(
        ISender sender,
        CatalogProblemDetailsFactory problemDetailsFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _sender = sender;
        _problemDetailsFactory = problemDetailsFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<PagedResult<ProductDto>> GetProductsAsync(
        int pageNumber,
        int pageSize,
        string? sortField,
        SortDirection sortDirection,
        string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            () => _sender.Send(
                new GetProductsQuery(pageNumber, pageSize, sortField, sortDirection, searchTerm),
                cancellationToken),
            "/api/products");
    }

    public Task<ProductDto> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            () => _sender.Send(new GetProductByIdQuery(productId), cancellationToken),
            $"/api/products/{productId:D}");
    }

    public Task<ProductDto> CreateProductAsync(
        ProductEditorModel model,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            () => _sender.Send(
                new CreateProductCommand(
                    model.Name,
                    model.Sku,
                    model.SalePrice,
                    model.Cost,
                    model.Stock,
                    requestId),
                cancellationToken),
            "/api/products");
    }

    public Task<ProductDto> UpdateProductAsync(
        Guid productId,
        ProductEditorModel model,
        ProductDto originalProduct,
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var stockDelta = model.Stock - originalProduct.Stock;
        var salePriceChanged = model.SalePrice != originalProduct.SalePrice;
        var costChanged = model.Cost != originalProduct.Cost;

        return ExecuteAsync(
            () => _sender.Send(
                new UpdateProductCommand(
                    productId,
                    salePriceChanged || costChanged ? model.SalePrice : null,
                    salePriceChanged || costChanged ? model.Cost : null,
                    stockDelta != 0 ? stockDelta : null,
                    requestId),
                cancellationToken),
            $"/api/products/{productId:D}");
    }

    public Task DeleteProductAsync(Guid productId, Guid requestId, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            () => _sender.Send(new DeleteProductCommand(productId, requestId), cancellationToken),
            $"/api/products/{productId:D}");
    }

    public Task<SkuExistsDto> CheckSkuExistsAsync(string sku, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            () => _sender.Send(new CheckSkuExistsQuery(sku), cancellationToken),
            "/api/products/sku-exists");
    }

    private async Task<T> ExecuteAsync<T>(Func<Task<T>> action, string instance)
    {
        try
        {
            return await action();
        }
        catch (CatalogProblemDetailsException)
        {
            throw;
        }
        catch (Exception exception)
        {
            var traceId = _httpContextAccessor.HttpContext?.TraceIdentifier ?? Activity.Current?.Id;
            var problemDetails = _problemDetailsFactory.CreateForException(instance, traceId, exception);
            throw new CatalogProblemDetailsException(problemDetails, exception);
        }
    }
}
