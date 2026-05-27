using ProductCatalog.Application.Products.Commands.UpdateProduct;

namespace ProductCatalog.UnitTests.Application.Commands.UpdateProduct;

public sealed class UpdateProductCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenNoChangeWasProvided()
    {
        var validator = new UpdateProductCommandValidator();

        var result = await validator.ValidateAsync(
            new UpdateProductCommand(Guid.NewGuid(), null, null, null));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenOnlyOnePriceFieldWasProvided()
    {
        var validator = new UpdateProductCommandValidator();

        var result = await validator.ValidateAsync(
            new UpdateProductCommand(Guid.NewGuid(), 135m, null, null));

        result.IsValid.Should().BeFalse();
    }
}
