using ProductCatalog.Application.Products.Commands.CreateProduct;

namespace ProductCatalog.UnitTests.Application.Commands.CreateProduct;

public sealed class CreateProductCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenSkuFormatIsInvalid()
    {
        var validator = new CreateProductCommandValidator();

        var result = await validator.ValidateAsync(
            new CreateProductCommand("Mechanical Keyboard", "??", 120m, 70m, 15));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(CreateProductCommand.Sku));
    }

    [Fact]
    public async Task ValidateAsync_ShouldPass_WhenRequestIsValid()
    {
        var validator = new CreateProductCommandValidator();

        var result = await validator.ValidateAsync(
            new CreateProductCommand("Mechanical Keyboard", "mkb--001", 120m, 70m, 15));

        result.IsValid.Should().BeTrue();
    }
}
