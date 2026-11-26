using WebAPI.Common.Results;

namespace WebAPI.Features.JsonPlaceholder;

internal static class JsonPlaceholderErrors
{
    public static readonly Error UserNotFound = new(
        "JsonPlaceholder.UserNotFound", "The JSONPlaceholder user was not found.", ErrorType.NotFound);
}
