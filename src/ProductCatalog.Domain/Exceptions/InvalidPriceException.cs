namespace ProductCatalog.Domain.Exceptions;

public sealed class InvalidPriceException : DomainException
{
    public InvalidPriceException(decimal salePrice, decimal cost)
        : base($"Sale price '{salePrice}' cannot be lower than cost '{cost}'.")
    {
    }
}
