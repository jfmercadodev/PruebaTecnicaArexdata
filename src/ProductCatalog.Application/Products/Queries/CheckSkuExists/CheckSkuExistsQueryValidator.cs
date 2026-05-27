using FluentValidation;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Application.Products.Queries.CheckSkuExists;

public sealed class CheckSkuExistsQueryValidator : AbstractValidator<CheckSkuExistsQuery>
{
    public CheckSkuExistsQueryValidator()
    {
        RuleFor(query => query.Sku)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeValidSku)
            .WithMessage("El formato del SKU es invalido.");
    }

    private static bool BeValidSku(string sku)
    {
        try
        {
            Sku.Create(sku);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
