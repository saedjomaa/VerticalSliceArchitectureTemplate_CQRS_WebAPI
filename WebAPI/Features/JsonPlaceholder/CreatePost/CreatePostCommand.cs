namespace WebAPI.Features.JsonPlaceholder.CreatePost;

public sealed record CreatePostCommand(int UserId, string Title, string Body)
{
    public const int MaximumTitleLength = 200;
    public const int MaximumBodyLength = 2000;
}
