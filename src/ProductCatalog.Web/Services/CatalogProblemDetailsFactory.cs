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

        problemDetails.Detail = _hostEnvironment.IsDevelopment()
            ? exception.Message
            : MapPublicDetail(exception);

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
            ValidationException => (StatusCodes.Status400BadRequest, "Validacion fallida"),
            NotFoundException => (StatusCodes.Status404NotFound, "Producto no encontrado"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflicto detectado"),
            InvalidPriceException => (StatusCodes.Status422UnprocessableEntity, "Precio invalido"),
            InvalidProductNameException => (StatusCodes.Status422UnprocessableEntity, "Nombre de producto invalido"),
            InvalidSkuException => (StatusCodes.Status422UnprocessableEntity, "SKU invalido"),
            InvalidStockException => (StatusCodes.Status422UnprocessableEntity, "Stock invalido"),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio incumplida"),
            _ => (StatusCodes.Status500InternalServerError, "Error inesperado")
        };
    }

    private static string MapPublicDetail(Exception exception)
    {
        return exception switch
        {
            ValidationException => "Revisa los datos enviados y corrige los campos resaltados.",
            NotFoundException => "No se encontro el producto solicitado.",
            ConflictException => "La solicitud entra en conflicto con el estado actual del catalogo.",
            InvalidPriceException => "El precio de venta debe ser mayor o igual al costo.",
            InvalidProductNameException => "El nombre del producto debe tener entre 3 y 200 caracteres.",
            InvalidSkuException => "El SKU debe tener entre 3 y 50 caracteres validos, letras, numeros o guiones.",
            InvalidStockException => "El stock no puede ser negativo.",
            DomainException => "La solicitud incumple una regla de negocio.",
            _ => "Ocurrio un error inesperado al procesar la solicitud."
        };
    }
}
