namespace ProductCatalog.Domain.Exceptions;

public sealed class InvalidSkuException : DomainException
{
    public InvalidSkuException(string message)
        : base(message)
    {
    }
}
