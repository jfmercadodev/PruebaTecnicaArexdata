using MediatR;

namespace ProductCatalog.Application.Common.Abstractions;

public interface IQuery<out TResponse> : IRequest<TResponse>;
