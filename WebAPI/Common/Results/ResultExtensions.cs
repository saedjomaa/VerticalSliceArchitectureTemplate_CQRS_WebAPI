using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebAPI.Common.Results;

internal static class ResultExtensions
{
    public static ObjectResult ToProblemDetails(this IResult result, HttpContext context)
    {
        var error = result.Error ?? throw new InvalidOperationException("A successful result has no error.");
        var factory = context.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.BadGateway => StatusCodes.Status502BadGateway,
            ErrorType.GatewayTimeout => StatusCodes.Status504GatewayTimeout,
            _ => throw new ArgumentOutOfRangeException(nameof(result), error.Type, "Unknown error type.")
        };

        ProblemDetails problem;
        if (error.ValidationErrors is { } validationErrors)
        {
            var modelState = new ModelStateDictionary();
            foreach (var (field, messages) in validationErrors)
            {
                foreach (var message in messages)
                {
                    modelState.AddModelError(field, message);
                }
            }

            problem = factory.CreateValidationProblemDetails(context, modelState, status, detail: error.Description);
        }
        else
        {
            problem = factory.CreateProblemDetails(context, status, detail: error.Description);
        }

        problem.Extensions["code"] = error.Code;
        return new ObjectResult(problem)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" }
        };
    }
}
