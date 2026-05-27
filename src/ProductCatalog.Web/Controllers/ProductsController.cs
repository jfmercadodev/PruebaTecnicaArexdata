using Microsoft.AspNetCore.Mvc;
using MediatR;
using ProductCatalog.Application.Products.Commands.CreateProduct;
using ProductCatalog.Application.Products.Commands.DeleteProduct;
using ProductCatalog.Application.Products.Commands.UpdateProduct;
using ProductCatalog.Application.Products.Queries.CheckSkuExists;
using ProductCatalog.Application.Products.Queries.GetProductById;
using ProductCatalog.Application.Products.Queries.GetProducts;
using ProductCatalog.Web.Contracts.Products;

namespace ProductCatalog.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private const string RequestIdHeaderName = "X-Request-Id";
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] GetProductsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new GetProductsQuery(
                request.PageNumber,
                request.PageSize,
                request.SortField,
                request.SortDirection,
                request.SearchTerm),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new GetProductByIdQuery(id), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductRequest request,
        [FromHeader(Name = RequestIdHeaderName)] Guid? requestId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new CreateProductCommand(
                request.Name,
                request.Sku,
                request.SalePrice,
                request.Cost,
                request.Stock,
                requestId),
            cancellationToken);

        return CreatedAtAction(nameof(GetProductById), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        [FromBody] UpdateProductRequest request,
        [FromHeader(Name = RequestIdHeaderName)] Guid? requestId,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new UpdateProductCommand(
                id,
                request.SalePrice,
                request.Cost,
                request.StockDelta,
                requestId),
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(
        Guid id,
        [FromHeader(Name = RequestIdHeaderName)] Guid? requestId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteProductCommand(id, requestId), cancellationToken);
        return NoContent();
    }

    [HttpGet("sku-exists")]
    public async Task<IActionResult> ExistsBySku([FromQuery] string sku, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(new CheckSkuExistsQuery(sku), cancellationToken);
        return Ok(response);
    }
}
