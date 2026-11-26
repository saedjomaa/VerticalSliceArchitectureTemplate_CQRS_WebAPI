using WebAPI.Common.Cqrs;
using WebAPI.Common.Results;
using WebAPI.Features.JsonPlaceholder.Client;

namespace WebAPI.Features.JsonPlaceholder.CreatePost;

internal sealed class CreatePostCommandHandler(IJsonPlaceholderClient jsonPlaceholder)
    : ICommandHandler<CreatePostCommand, Result<JsonPlaceholderPostResponse>>
{
    public Task<Result<JsonPlaceholderPostResponse>> HandleAsync(
        CreatePostCommand command, CancellationToken cancellationToken) =>
        jsonPlaceholder.CreatePostAsync(command.UserId, command.Title, command.Body, cancellationToken);
}
