using Microsoft.AspNetCore.Mvc;
using WebAPI.Common.Cqrs;
using WebAPI.Common.Pagination;
using WebAPI.Common.Results;
using WebAPI.Features.JsonPlaceholder.CreatePost;
using WebAPI.Features.JsonPlaceholder.GetUser;
using WebAPI.Features.JsonPlaceholder.ListUsers;

namespace WebAPI.Features.JsonPlaceholder;

/// <summary>
/// Endpoints backed by the external JSONPlaceholder API (https://jsonplaceholder.typicode.com).
/// One controller per external service; the database is not used here.
/// </summary>
[ApiController]
[Route("api/jsonplaceholder")]
public sealed class JsonPlaceholderController : ControllerBase
{
    [HttpGet("users")]
    [EndpointSummary("List users from JSONPlaceholder")]
    [ProducesResponseType<PagedResult<JsonPlaceholderUserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<PagedResult<JsonPlaceholderUserResponse>>> ListUsers(
        [FromServices] IQueryHandler<ListUsersQuery, Result<PagedResult<JsonPlaceholderUserResponse>>> handler,
        CancellationToken cancellationToken,
        [FromQuery] int page = Pagination.DefaultPage,
        [FromQuery] int pageSize = Pagination.DefaultPageSize)
    {
        var result = await handler.HandleAsync(new ListUsersQuery(page, pageSize), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }

    [HttpGet("users/{id:int}")]
    [EndpointSummary("Get a user from JSONPlaceholder")]
    [ProducesResponseType<JsonPlaceholderUserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<JsonPlaceholderUserResponse>> GetUser(
        int id,
        [FromServices] IQueryHandler<GetUserQuery, Result<JsonPlaceholderUserResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetUserQuery(id), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails(HttpContext);
    }

    [HttpPost("posts")]
    [EndpointSummary("Create a post in JSONPlaceholder (fake: JSONPlaceholder does not persist it)")]
    [ProducesResponseType<JsonPlaceholderPostResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status504GatewayTimeout)]
    public async Task<ActionResult<JsonPlaceholderPostResponse>> CreatePost(
        CreatePostCommand command,
        [FromServices] ICommandHandler<CreatePostCommand, Result<JsonPlaceholderPostResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);

        // There is no endpoint to read a single post, so the 201 has no Location header.
        return result.IsSuccess
            ? Created((string?)null, result.Value)
            : result.ToProblemDetails(HttpContext);
    }
}
