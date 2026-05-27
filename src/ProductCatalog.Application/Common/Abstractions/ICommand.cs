using MediatR;

namespace ProductCatalog.Application.Common.Abstractions;

public interface ICommand<out TResponse> : IRequest<TResponse>;
