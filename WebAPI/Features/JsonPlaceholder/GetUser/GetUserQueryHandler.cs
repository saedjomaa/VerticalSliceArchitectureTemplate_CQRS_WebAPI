using WebAPI.Common.Cqrs;
using WebAPI.Common.ExternalServices;
using WebAPI.Common.Results;
using WebAPI.Features.JsonPlaceholder.Client;

namespace WebAPI.Features.JsonPlaceholder.GetUser;

internal sealed class GetUserQueryHandler(IJsonPlaceholderClient jsonPlaceholder)
    : IQueryHandler<GetUserQuery, Result<JsonPlaceholderUserResponse>>
{
    public async Task<Result<JsonPlaceholderUserResponse>> HandleAsync(
        GetUserQuery query, CancellationToken cancellationToken)
    {
        var result = await jsonPlaceholder.GetUserAsync(query.Id, cancellationToken);

        return result.Error == ExternalServiceErrors.NotFound
            ? Result<JsonPlaceholderUserResponse>.Failure(JsonPlaceholderErrors.UserNotFound)
            : result;
    }
}
