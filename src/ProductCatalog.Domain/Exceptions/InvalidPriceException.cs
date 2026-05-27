namespace ProductCatalog.Domain.Exceptions;

public sealed class InvalidPriceException : DomainException
{
    public InvalidPriceException(decimal salePrice, decimal cost)
        : base($"El precio de venta '{salePrice}' no puede ser menor que el costo '{cost}'.")
    {
    }
}
