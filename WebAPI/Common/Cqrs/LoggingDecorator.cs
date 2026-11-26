using System.Diagnostics;
using WebAPI.Common.Results;

namespace WebAPI.Common.Cqrs;

internal static class LoggingDecorator
{
    internal sealed class Command<TCommand, TResult>(
        ICommandHandler<TCommand, TResult> inner,
        ILogger<Command<TCommand, TResult>> logger) : ICommandHandler<TCommand, TResult>
        where TResult : IResult<TResult>
    {
        public Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken) =>
            LogAsync(command, inner.HandleAsync, logger, cancellationToken);
    }

    internal sealed class Query<TQuery, TResult>(
        IQueryHandler<TQuery, TResult> inner,
        ILogger<Query<TQuery, TResult>> logger) : IQueryHandler<TQuery, TResult>
        where TResult : IResult<TResult>
    {
        public Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken) =>
            LogAsync(query, inner.HandleAsync, logger, cancellationToken);
    }

    private static async Task<TResult> LogAsync<TRequest, TResult>(
        TRequest request,
        Func<TRequest, CancellationToken, Task<TResult>> handle,
        ILogger logger,
        CancellationToken cancellationToken) where TResult : IResult<TResult>
    {
        var started = Stopwatch.GetTimestamp();
        var result = await handle(request, cancellationToken);

        logger.LogInformation(
            "Handled {RequestType} in {ElapsedMs} ms with success {IsSuccess} and error {ErrorCode}",
            typeof(TRequest).Name, Stopwatch.GetElapsedTime(started).TotalMilliseconds,
            result.IsSuccess, result.Error?.Code);

        return result;
    }
}
