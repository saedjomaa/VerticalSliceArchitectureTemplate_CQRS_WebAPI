using WebAPI.Common.Results;

namespace WebAPI.Common.Cqrs;

public interface ICommandHandler<TCommand, TResult> where TResult : IResult<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
