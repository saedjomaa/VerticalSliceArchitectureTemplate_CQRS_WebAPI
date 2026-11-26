using WebAPI.Common.Results;

namespace WebAPI.Common.ExternalServices;

/// <summary>
/// Errors returned by external service clients. Handlers translate <see cref="NotFound"/>
/// to their module's own error; the other errors are returned to the client as they are.
/// </summary>
public static class ExternalServiceErrors
{
    public static readonly Error NotFound = new(
        "ExternalService.NotFound", "The requested resource was not found in the external service.", ErrorType.NotFound);

    public static readonly Error Unavailable = new(
        "ExternalService.Unavailable", "The external service is unavailable. Try again later.", ErrorType.BadGateway);

    public static readonly Error Timeout = new(
        "ExternalService.Timeout", "The external service did not respond in time. Try again later.", ErrorType.GatewayTimeout);

    public static readonly Error InvalidResponse = new(
        "ExternalService.InvalidResponse", "The external service returned an invalid response.", ErrorType.BadGateway);
}
