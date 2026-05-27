namespace ProductCatalog.Application.Common.Abstractions;

public interface IIdempotentCommand<out TResponse> : ICommand<TResponse>
{
    Guid? RequestId { get; }

    TimeSpan IdempotencyWindow { get; }
}
