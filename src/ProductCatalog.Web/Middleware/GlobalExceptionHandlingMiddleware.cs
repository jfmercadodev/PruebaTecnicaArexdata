using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Web.Services;

namespace ProductCatalog.Web.Middleware;

public sealed class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private readonly CatalogProblemDetailsFactory _problemDetailsFactory;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        CatalogProblemDetailsFactory problemDetailsFactory,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _problemDetailsFactory = problemDetailsFactory;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            try
            {
                _logger.LogError(exception, "Unhandled exception while processing request {Method} {Path}", context.Request.Method, context.Request.Path);
            }
            catch
            {
            }

            var problemDetails = _problemDetailsFactory.CreateForException(context, exception);

            context.Response.Clear();
            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            context.Response.Headers[CorrelationIdMiddleware.HeaderName] = context.TraceIdentifier;

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails), context.RequestAborted);
        }
    }
}
