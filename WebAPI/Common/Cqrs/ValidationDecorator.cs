using FluentValidation;
using FluentValidation.Results;
using WebAPI.Common.Results;

namespace WebAPI.Common.Cqrs;

internal static class ValidationDecorator
{
    internal sealed class Command<TCommand, TResult>(
        ICommandHandler<TCommand, TResult> inner,
        IEnumerable<IValidator<TCommand>> validators) : ICommandHandler<TCommand, TResult>
        where TResult : IResult<TResult>
    {
        public Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken) =>
            ValidateAsync(command, validators, inner.HandleAsync, cancellationToken);
    }

    internal sealed class Query<TQuery, TResult>(
        IQueryHandler<TQuery, TResult> inner,
        IEnumerable<IValidator<TQuery>> validators) : IQueryHandler<TQuery, TResult>
        where TResult : IResult<TResult>
    {
        public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken) =>
            ValidateAsync(query, validators, inner.HandleAsync, cancellationToken);
    }

    private static async Task<TResult> ValidateAsync<TRequest, TResult>(
        TRequest request,
        IEnumerable<IValidator<TRequest>> validators,
        Func<TRequest, CancellationToken, Task<TResult>> handle,
        CancellationToken cancellationToken) where TResult : IResult<TResult>
    {
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(request, cancellationToken);
            failures.AddRange(result.Errors);
        }

        if (failures.Count == 0)
        {
            return await handle(request, cancellationToken);
        }

        var errors = failures.GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).Distinct().ToArray());

        return TResult.Failure(new Error(
            "Validation.Failed", "One or more fields are invalid.", ErrorType.Validation, errors));
    }
}
