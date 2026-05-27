using ProductCatalog.Application.Products.Commands.UpdateProduct;

namespace ProductCatalog.UnitTests.Application.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenNoChangeWasProvided()
    {
        var validator = new UpdateProductCommandValidator();

        var result = await validator.ValidateAsync(
            new UpdateProductCommand(Guid.NewGuid(), null, null, null, null, null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenOnlyOnePriceFieldWasProvided()
    {
        var validator = new UpdateProductCommandValidator();

        var result = await validator.ValidateAsync(
            new UpdateProductCommand(Guid.NewGuid(), null, null, 135m, null, null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_ShouldPass_WhenOnlyNameWasProvided()
    {
        var validator = new UpdateProductCommandValidator();

        var result = await validator.ValidateAsync(
            new UpdateProductCommand(Guid.NewGuid(), "Nuevo nombre", null, null, null, null));

        result.IsValid.Should().BeTrue();
    }
}
