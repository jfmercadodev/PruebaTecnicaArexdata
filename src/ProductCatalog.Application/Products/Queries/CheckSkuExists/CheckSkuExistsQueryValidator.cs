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
            .WithMessage("SKU format is invalid.");
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
