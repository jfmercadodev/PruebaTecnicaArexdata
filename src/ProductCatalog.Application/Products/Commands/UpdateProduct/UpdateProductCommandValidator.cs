using FluentValidation;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.ProductId)
            .NotEmpty();

        RuleFor(command => command)
            .Must(HaveAtLeastOneChange)
            .WithMessage("La solicitud de actualizacion debe incluir nombre, SKU, precio/costo y/o delta de stock.");

        RuleFor(command => command)
            .Must(HaveCompletePricePair)
            .WithMessage("SalePrice y Cost deben enviarse juntos.");

        When(command => command.SalePrice.HasValue, () =>
        {
            RuleFor(command => command.SalePrice!.Value)
                .GreaterThan(0);
        });

        When(command => command.Cost.HasValue, () =>
        {
            RuleFor(command => command.Cost!.Value)
                .GreaterThan(0);
        });

        When(command => command.Name is not null, () =>
        {
            RuleFor(command => command.Name!)
                .NotEmpty()
                .Length(3, 200);
        });

        When(command => command.Sku is not null, () =>
        {
            RuleFor(command => command.Sku!)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeValidSku)
                .WithMessage("El formato del SKU es invalido.")
                .Length(3, 50);
        });
    }

    private static bool HaveAtLeastOneChange(UpdateProductCommand command)
    {
        return command.Name is not null
            || command.Sku is not null
            || command.StockDelta.HasValue
            || command.SalePrice.HasValue
            || command.Cost.HasValue;
    }

    private static bool HaveCompletePricePair(UpdateProductCommand command)
    {
        return command.SalePrice.HasValue == command.Cost.HasValue;
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
