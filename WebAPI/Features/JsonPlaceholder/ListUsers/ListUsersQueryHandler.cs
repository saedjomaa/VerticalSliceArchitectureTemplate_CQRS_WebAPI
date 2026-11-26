using WebAPI.Common.Cqrs;
using WebAPI.Common.Results;
using WebAPI.Features.JsonPlaceholder.Client;

namespace WebAPI.Features.JsonPlaceholder.ListUsers;

internal sealed class ListUsersQueryHandler(IJsonPlaceholderClient jsonPlaceholder)
    : IQueryHandler<ListUsersQuery, Result<PagedResult<JsonPlaceholderUserResponse>>>
{
    public Task<Result<PagedResult<JsonPlaceholderUserResponse>>> HandleAsync(
        ListUsersQuery query, CancellationToken cancellationToken) =>
        jsonPlaceholder.ListUsersAsync(query.Page, query.PageSize, cancellationToken);
}
