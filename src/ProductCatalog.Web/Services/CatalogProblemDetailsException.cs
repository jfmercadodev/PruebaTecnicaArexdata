using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog.Web.Services;

public sealed class CatalogProblemDetailsException : Exception
{
    public CatalogProblemDetailsException(ProblemDetails problemDetails, Exception innerException)
        : base(problemDetails.Title, innerException)
    {
        ProblemDetails = problemDetails;
    }

    public ProblemDetails ProblemDetails { get; }
}
