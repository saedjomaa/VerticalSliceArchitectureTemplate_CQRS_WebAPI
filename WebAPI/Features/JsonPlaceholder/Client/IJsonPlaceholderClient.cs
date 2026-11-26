using WebAPI.Common.Results;
using WebAPI.Features.JsonPlaceholder.CreatePost;

namespace WebAPI.Features.JsonPlaceholder.Client;

/// <summary>
/// Client for the external JSONPlaceholder API, used only by this module's handlers.
/// Every expected failure (not found, timeout, service unavailable, invalid response) is returned
/// as a failed <see cref="Result"/> with an error from <c>ExternalServiceErrors</c>; nothing is thrown.
/// The interface lets handlers be tested with a fake client.
/// </summary>
internal interface IJsonPlaceholderClient
{
    Task<Result<PagedResult<JsonPlaceholderUserResponse>>> ListUsersAsync(
        int page, int pageSize, CancellationToken cancellationToken);

    Task<Result<JsonPlaceholderUserResponse>> GetUserAsync(int id, CancellationToken cancellationToken);

    Task<Result<JsonPlaceholderPostResponse>> CreatePostAsync(
        int userId, string title, string body, CancellationToken cancellationToken);
}
