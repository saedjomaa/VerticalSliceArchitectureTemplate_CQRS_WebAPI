using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace WebAPI.Common.Diagnostics;

/// <summary>
/// Logs the exact ProblemDetails JSON returned to the client whenever a controller action fails
/// (expected errors from <c>ToProblemDetails</c>, validation errors, model binding errors).
/// Successful responses are not logged. Unexpected exceptions (500) are logged by <see cref="GlobalExceptionHandler"/>.
/// </summary>
internal sealed class ProblemDetailsLoggingFilter(
    ILogger<ProblemDetailsLoggingFilter> logger,
    IOptions<JsonOptions> jsonOptions) : IAsyncAlwaysRunResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: ProblemDetails problem } result)
        {
            var statusCode = result.StatusCode ?? problem.Status ?? StatusCodes.Status500InternalServerError;
            var json = JsonSerializer.Serialize(problem, problem.GetType(), jsonOptions.Value.JsonSerializerOptions);

            logger.Log(
                statusCode >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning,
                "Responded {StatusCode} with {ProblemDetails}",
                statusCode,
                json);
        }

        return next();
    }
}
