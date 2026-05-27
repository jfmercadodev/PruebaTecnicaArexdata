namespace ProductCatalog.Domain.Exceptions;

public sealed class InvalidStockException : DomainException
{
    public InvalidStockException(int stock)
        : base($"El stock '{stock}' no puede ser negativo.")
    {
    }
}
