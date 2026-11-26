using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebAPI.Common.ExternalServices;
using WebAPI.Common.Results;
using WebAPI.Features.JsonPlaceholder.CreatePost;

namespace WebAPI.Features.JsonPlaceholder.Client;

/// <summary>
/// Typed HttpClient for https://jsonplaceholder.typicode.com. Base address and timeout come from
/// <see cref="JsonPlaceholderOptions"/> and <see cref="ExternalServicesOptions"/> (configured in <c>JsonPlaceholderModule</c>).
/// All HTTP, timeout and JSON failures are translated to <see cref="ExternalServiceErrors"/> in
/// <see cref="SendAsync{T}"/>; only a cancellation requested by the caller is rethrown.
/// </summary>
internal sealed class JsonPlaceholderClient(HttpClient http, ILogger<JsonPlaceholderClient> logger)
    : IJsonPlaceholderClient
{
    private const string TotalCountHeader = "X-Total-Count";
    private static readonly JsonSerializerOptions Json = JsonSerializerOptions.Web;

    public Task<Result<PagedResult<JsonPlaceholderUserResponse>>> ListUsersAsync(
        int page, int pageSize, CancellationToken cancellationToken) =>
        SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"users?_page={page}&_limit={pageSize}"),
            async (response, token) =>
            {
                var users = await response.Content.ReadFromJsonAsync<List<JsonPlaceholderUser>>(Json, token);
                if (users is null || !TryGetTotalCount(response, out var totalCount))
                {
                    return null;
                }

                return new PagedResult<JsonPlaceholderUserResponse>(
                    users.Select(ToResponse).ToList(), page, pageSize, totalCount);
            },
            cancellationToken);

    public Task<Result<JsonPlaceholderUserResponse>> GetUserAsync(int id, CancellationToken cancellationToken) =>
        SendAsync(
            new HttpRequestMessage(HttpMethod.Get, $"users/{id}"),
            async (response, token) =>
            {
                var user = await response.Content.ReadFromJsonAsync<JsonPlaceholderUser>(Json, token);
                return user is null ? null : ToResponse(user);
            },
            cancellationToken);

    public Task<Result<JsonPlaceholderPostResponse>> CreatePostAsync(
        int userId, string title, string body, CancellationToken cancellationToken) =>
        SendAsync(
            new HttpRequestMessage(HttpMethod.Post, "posts")
            {
                Content = JsonContent.Create(new JsonPlaceholderNewPost(userId, title, body), options: Json)
            },
            async (response, token) =>
            {
                var post = await response.Content.ReadFromJsonAsync<JsonPlaceholderPost>(Json, token);
                return post is null
                    ? null
                    : new JsonPlaceholderPostResponse(post.Id, post.UserId, post.Title ?? string.Empty, post.Body ?? string.Empty);
            },
            cancellationToken);

    /// <summary>
    /// Sends the request and converts every expected failure into a failed <see cref="Result{T}"/>.
    /// <paramref name="read"/> returns <c>null</c> when the body is missing or incomplete.
    /// </summary>
    private async Task<Result<T>> SendAsync<T>(
        HttpRequestMessage request,
        Func<HttpResponseMessage, CancellationToken, Task<T?>> read,
        CancellationToken cancellationToken) where T : class
    {
        using var _ = request;
        try
        {
            using var response = await http.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return Result<T>.Failure(ExternalServiceErrors.NotFound);
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "JSONPlaceholder {Method} {Uri} returned {StatusCode}",
                    request.Method, request.RequestUri, (int)response.StatusCode);
                return Result<T>.Failure(ExternalServiceErrors.Unavailable);
            }

            var value = await read(response, cancellationToken);
            if (value is null)
            {
                logger.LogWarning(
                    "JSONPlaceholder {Method} {Uri} returned an empty or incomplete response",
                    request.Method, request.RequestUri);
                return Result<T>.Failure(ExternalServiceErrors.InvalidResponse);
            }

            return Result<T>.Success(value);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            // HttpClient.Timeout elapsed. A cancellation requested by the caller is not caught here.
            logger.LogWarning(
                "JSONPlaceholder {Method} {Uri} timed out after {TimeoutSeconds} s",
                request.Method, request.RequestUri, http.Timeout.TotalSeconds);
            return Result<T>.Failure(ExternalServiceErrors.Timeout);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(
                exception, "JSONPlaceholder {Method} {Uri} failed", request.Method, request.RequestUri);
            return Result<T>.Failure(ExternalServiceErrors.Unavailable);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(
                exception, "JSONPlaceholder {Method} {Uri} returned invalid JSON", request.Method, request.RequestUri);
            return Result<T>.Failure(ExternalServiceErrors.InvalidResponse);
        }
    }

    private static bool TryGetTotalCount(HttpResponseMessage response, out int totalCount)
    {
        totalCount = 0;
        return response.Headers.TryGetValues(TotalCountHeader, out var values)
            && int.TryParse(values.FirstOrDefault(), out totalCount);
    }

    private static JsonPlaceholderUserResponse ToResponse(JsonPlaceholderUser user) => new(
        user.Id,
        user.Name ?? string.Empty,
        user.Username ?? string.Empty,
        user.Email ?? string.Empty,
        user.Phone ?? string.Empty,
        user.Website ?? string.Empty);
}
