using FluentValidation;
using ProductCatalog.Application.Common.Behaviors;
using ProductCatalog.Application.Products.Commands.CreateProduct;

namespace ProductCatalog.UnitTests.Application.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ShouldThrowValidationException_WhenValidatorFails()
    {
        var validators = new[]
        {
            new CreateProductCommandValidator()
        };

        var behavior = new ValidationBehavior<CreateProductCommand, string>(validators);

        var action = async () => await behavior.Handle(
            new CreateProductCommand("", "", 0m, 0m, -1),
            () => Task.FromResult("ok"),
            CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_ShouldCallNext_WhenRequestIsValid()
    {
        var validators = new[]
        {
            new CreateProductCommandValidator()
        };

        var behavior = new ValidationBehavior<CreateProductCommand, string>(validators);

        var response = await behavior.Handle(
            new CreateProductCommand("Mechanical Keyboard", "MKB-001", 120m, 70m, 15),
            () => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
    }
}
