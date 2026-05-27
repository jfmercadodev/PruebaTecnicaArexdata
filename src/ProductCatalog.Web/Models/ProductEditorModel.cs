using System.ComponentModel.DataAnnotations;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Web.Models;

public sealed class ProductEditorModel : IValidatableObject
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "El nombre del producto debe tener entre 3 y 200 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    public string Sku { get; set; } = string.Empty;

    public decimal SalePrice { get; set; }

    public decimal Cost { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    public static ProductEditorModel FromProduct(ProductDto product)
    {
        return new ProductEditorModel
        {
            Name = product.Name,
            Sku = product.Sku,
            SalePrice = product.SalePrice,
            Cost = product.Cost,
            Stock = product.Stock
        };
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SalePrice <= 0)
        {
            yield return new ValidationResult(
                "El precio de venta debe ser mayor que cero.",
                [nameof(SalePrice)]);
        }

        if (Cost <= 0)
        {
            yield return new ValidationResult(
                "El costo debe ser mayor que cero.",
                [nameof(Cost)]);
        }
    }
}
