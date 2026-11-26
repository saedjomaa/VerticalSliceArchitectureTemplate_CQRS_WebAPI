using WebAPI.Common.Results;

namespace WebAPI.Common.Cqrs;

public interface IQueryHandler<TQuery, TResult> where TResult : IResult<TResult>
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}
