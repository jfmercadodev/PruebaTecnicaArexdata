using System.ComponentModel.DataAnnotations;
using ProductCatalog.Application.Products.Dtos;

namespace ProductCatalog.Web.Models;

public sealed class ProductEditorModel : IValidatableObject
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "SKU is required.")]
    public string Sku { get; set; } = string.Empty;

    public decimal SalePrice { get; set; }

    public decimal Cost { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
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
                "Sale price must be greater than zero.",
                [nameof(SalePrice)]);
        }

        if (Cost <= 0)
        {
            yield return new ValidationResult(
                "Cost must be greater than zero.",
                [nameof(Cost)]);
        }
    }
}
