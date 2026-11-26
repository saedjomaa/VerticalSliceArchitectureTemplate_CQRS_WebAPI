namespace WebAPI.Common.ExternalServices;

/// <summary>Settings shared by every external service client (section <c>ExternalServices</c>).</summary>
internal sealed class ExternalServicesOptions
{
    public const string SectionName = "ExternalServices";

    /// <summary>Maximum time for one request to any external service, including reading the response body.</summary>
    public int TimeoutSeconds { get; set; } = 10;
}
