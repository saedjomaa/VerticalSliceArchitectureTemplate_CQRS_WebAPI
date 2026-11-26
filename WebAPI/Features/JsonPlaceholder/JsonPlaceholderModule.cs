using Microsoft.Extensions.Options;
using WebAPI.Common.ExternalServices;
using WebAPI.Features.JsonPlaceholder.Client;

namespace WebAPI.Features.JsonPlaceholder;

/// <summary>
/// Services this module needs besides its handlers and validators (those are registered by the scan in
/// <c>AddFeatures</c>). A module registers its own services here, so adding or removing it touches one folder.
/// </summary>
internal static class JsonPlaceholderModule
{
    public static IServiceCollection AddJsonPlaceholderModule(this IServiceCollection services)
    {
        // Not added to /health: an external outage does not mean this application is unhealthy.
        services.AddOptions<JsonPlaceholderOptions>().BindConfiguration(JsonPlaceholderOptions.SectionName);
        services.AddHttpClient<IJsonPlaceholderClient, JsonPlaceholderClient>((provider, client) =>
        {
            client.BaseAddress = new Uri(provider.GetRequiredService<IOptions<JsonPlaceholderOptions>>().Value.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(
                provider.GetRequiredService<IOptions<ExternalServicesOptions>>().Value.TimeoutSeconds);
        });

        return services;
    }
}
