using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

namespace WebAPI.Common.Diagnostics;

internal sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    ProblemDetailsFactory problemDetailsFactory,
    IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions> jsonOptions) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unexpected exception occurred while handling the request");

        const int statusCode = StatusCodes.Status500InternalServerError;
        var problem = problemDetailsFactory.CreateProblemDetails(
            httpContext, statusCode, detail: "An unexpected error occurred.");

        logger.LogError(
            "Responded {StatusCode} with {ProblemDetails}",
            statusCode,
            JsonSerializer.Serialize(problem, jsonOptions.Value.SerializerOptions));

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            problem, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
