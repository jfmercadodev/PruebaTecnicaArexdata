using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace ProductCatalog.Web.Services;

public static class ProblemDetailsMessageFormatter
{
    public static string ToUserMessage(ProblemDetails problemDetails)
    {
        var validationMessage = TryGetValidationMessage(problemDetails);
        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return validationMessage;
        }

        if (!string.IsNullOrWhiteSpace(problemDetails.Detail))
        {
            return problemDetails.Detail;
        }

        return problemDetails.Title ?? "Error inesperado";
    }

    private static string? TryGetValidationMessage(ProblemDetails problemDetails)
    {
        if (!problemDetails.Extensions.TryGetValue("errors", out var errors) || errors is null)
        {
            return null;
        }

        if (errors is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var item in property.Value.EnumerateArray())
                {
                    var message = item.GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return message;
                    }
                }
            }
        }

        return null;
    }
}
