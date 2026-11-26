namespace WebAPI.Features.JsonPlaceholder.Client;

// Shapes of the external API's JSON. They never leave the Client folder: the client maps them
// to the module's response records. Fields we do not use (address, company) are ignored.

internal sealed record JsonPlaceholderUser(
    int Id, string? Name, string? Username, string? Email, string? Phone, string? Website);

internal sealed record JsonPlaceholderPost(int Id, int UserId, string? Title, string? Body);

internal sealed record JsonPlaceholderNewPost(int UserId, string Title, string Body);
