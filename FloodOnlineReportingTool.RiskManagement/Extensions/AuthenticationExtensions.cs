using FloodOnlineReportingTool.RiskManagement.Authorization;
using FloodOnlineReportingTool.RiskManagement.Options;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

namespace Microsoft.AspNetCore.Builder;

internal static class AuthenticationExtensions
{
    /// <summary>
    /// Configures authentication, authorization and policies.
    internal static IServiceCollection AddRiskManagementAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration
            .GetSection("FORT")
            .GetSection(KeyVaultAzureAdOptions.SectionName);

        // Setup Authentication
        services
            .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(section);

        ConfigureResilientOpenIdConnect(services);

        // Add Blazor cascading authentication state
        services.AddCascadingAuthenticationState();

        // Setup Authorization
        services
            .AddAuthorizationBuilder()
            .AddFallbackPolicy(PolicyNames.Admin, options =>
            {
                options.RequireAuthenticatedUser();
            });

        return services;
    }

    /// <summary>
    /// Configure a resilient OpenID Connect authentication handler.
    /// </summary>
    /// <remarks>For example, retrying when .well-known fails to read.</remarks>
    private static void ConfigureResilientOpenIdConnect(IServiceCollection services)
    {
        const string clientName = "OAuthResilient";

        services
            .AddHttpClient(clientName)
            .AddStandardResilienceHandler();

        services
            .AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<IHttpClientFactory>((options, httpClientFactory) =>
            {
                options.Backchannel = httpClientFactory.CreateClient(clientName);
            });
    }
}
