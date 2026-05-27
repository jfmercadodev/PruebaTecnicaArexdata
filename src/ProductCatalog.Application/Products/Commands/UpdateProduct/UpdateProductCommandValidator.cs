using FluentValidation;

namespace ProductCatalog.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(command => command.ProductId)
            .NotEmpty();

        RuleFor(command => command)
            .Must(HaveAtLeastOneChange)
            .WithMessage("Update request must include price/cost and/or stock delta.");

        RuleFor(command => command)
            .Must(HaveCompletePricePair)
            .WithMessage("SalePrice and Cost must be sent together.");

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
    }

    private static bool HaveAtLeastOneChange(UpdateProductCommand command)
    {
        return command.StockDelta.HasValue || command.SalePrice.HasValue || command.Cost.HasValue;
    }

    private static bool HaveCompletePricePair(UpdateProductCommand command)
    {
        return command.SalePrice.HasValue == command.Cost.HasValue;
    }
}
