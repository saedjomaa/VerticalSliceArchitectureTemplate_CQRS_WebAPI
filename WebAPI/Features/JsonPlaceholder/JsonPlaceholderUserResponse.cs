namespace WebAPI.Features.JsonPlaceholder;

public sealed record JsonPlaceholderUserResponse(
    int Id, string Name, string Username, string Email, string Phone, string Website);
