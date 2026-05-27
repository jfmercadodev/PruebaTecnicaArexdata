using FluentValidation;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .Length(3, 200);

        RuleFor(command => command.Sku)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(BeValidSku)
            .WithMessage("El formato del SKU es invalido.")
            .Length(3, 50);

        RuleFor(command => command.SalePrice)
            .GreaterThan(0);

        RuleFor(command => command.Cost)
            .GreaterThan(0);

        RuleFor(command => command.Stock)
            .GreaterThanOrEqualTo(0);
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
