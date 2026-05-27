namespace ProductCatalog.Domain.Exceptions;

public sealed class InvalidStockException : DomainException
{
    public InvalidStockException(int stock)
        : base($"Stock '{stock}' cannot be negative.")
    {
    }
}
