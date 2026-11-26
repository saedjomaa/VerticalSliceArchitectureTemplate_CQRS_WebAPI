namespace WebAPI.Features.JsonPlaceholder.Client;

internal sealed class JsonPlaceholderOptions
{
    public const string SectionName = "ExternalServices:JsonPlaceholder";

    /// <summary>Absolute URL ending with '/', e.g. https://jsonplaceholder.typicode.com/</summary>
    public string BaseUrl { get; set; } = string.Empty;
}
