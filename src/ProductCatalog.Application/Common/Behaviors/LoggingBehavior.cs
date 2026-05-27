using MediatR;
using Microsoft.Extensions.Logging;

namespace ProductCatalog.Application.Common.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTime.UtcNow;
        _logger.LogInformation("Handling request {RequestName}", typeof(TRequest).Name);

        try
        {
            var response = await next();
            var duration = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            _logger.LogInformation(
                "Handled request {RequestName} with success in {DurationMs}ms",
                typeof(TRequest).Name,
                duration);
            return response;
        }
        catch (Exception ex)
        {
            var duration = (long)(DateTime.UtcNow - startedAt).TotalMilliseconds;
            _logger.LogError(
                ex,
                "Handled request {RequestName} with error in {DurationMs}ms",
                typeof(TRequest).Name,
                duration);
            throw;
        }
    }
}
