using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.Common.Exceptions;
using ProductCatalog.Domain.Exceptions;

namespace ProductCatalog.Web.Services;

public sealed class CatalogProblemDetailsFactory
{
    private readonly IHostEnvironment _hostEnvironment;

    public CatalogProblemDetailsFactory(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public ProblemDetails CreateForException(HttpContext context, Exception exception)
    {
        return CreateForException(
            context.Request.Path,
            context.TraceIdentifier,
            exception);
    }

    public ProblemDetails CreateForException(string instance, string? traceId, Exception exception)
    {
        var (statusCode, title) = MapException(exception);
        var correlationId = string.IsNullOrWhiteSpace(traceId)
            ? Activity.Current?.Id ?? Guid.NewGuid().ToString("D")
            : traceId;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = instance
        };

        if (_hostEnvironment.IsDevelopment())
        {
            problemDetails.Detail = exception.Message;
        }

        problemDetails.Extensions["traceId"] = correlationId;
        problemDetails.Extensions["correlationId"] = correlationId;

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
        }

        return problemDetails;
    }

    private static (int StatusCode, string Title) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            NotFoundException => (StatusCodes.Status404NotFound, "Product not found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict detected"),
            InvalidPriceException => (StatusCodes.Status422UnprocessableEntity, "Invalid price"),
            InvalidProductNameException => (StatusCodes.Status422UnprocessableEntity, "Invalid product name"),
            InvalidSkuException => (StatusCodes.Status422UnprocessableEntity, "Invalid sku"),
            InvalidStockException => (StatusCodes.Status422UnprocessableEntity, "Invalid stock"),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Business rule violation"),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error")
        };
    }
}
