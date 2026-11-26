namespace WebAPI.Features.JsonPlaceholder.CreatePost;

public sealed record JsonPlaceholderPostResponse(int Id, int UserId, string Title, string Body);
