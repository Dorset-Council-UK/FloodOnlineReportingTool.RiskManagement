using FloodOnlineReportingTool.RiskManagement.Authorization;
using FloodOnlineReportingTool.RiskManagement.Endpoints.Account;
using FloodOnlineReportingTool.RiskManagement.Options;
using FloodOnlineReportingTool.RiskManagement.Settings;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class AuthenticationExtensions
{
    /// <summary>
    /// Configures authentication, authorization and policies.
    /// </summary>
    internal static TBuilder AddAuthentication<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var azureAdSection = builder.Configuration
            .GetRequiredSection("FORT")
            .GetRequiredSection(KeyVaultAzureAdOptions.SectionName);

        var reportStatusApiSection = builder.Configuration
            .GetRequiredSection(RiskManagementSettings.SectionName)
            .GetRequiredSection(RiskManagementSettings.ReportStatusApi);

        // Setup Authentication
        builder.Services
            .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(azureAdSection)
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddDownstreamApi(RiskManagementSettings.ReportStatusApi, reportStatusApiSection)
            .AddInMemoryTokenCaches();

        ConfigureResilientOpenIdConnect(builder.Services);

        // Add Blazor cascading authentication state
        builder.Services.AddCascadingAuthenticationState();

        // Setup Authorization
        var fallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        builder.Services
            .AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.Reader, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.IsInRole(RoleNames.Reader) ||
                    context.User.IsInRole(RoleNames.Writer) ||
                    context.User.IsInRole(RoleNames.Admin)))
            .AddPolicy(PolicyNames.Writer, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context =>
                    context.User.IsInRole(RoleNames.Writer) ||
                    context.User.IsInRole(RoleNames.Admin)))
            .AddPolicy(PolicyNames.Admin, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(RoleNames.Admin))
            .SetFallbackPolicy(fallbackPolicy);

        return builder;
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

    internal static WebApplication MapAuthenticationEndpoints(this WebApplication app)
    {
        app.MapGet("signin", AccountEndpoints.SignIn)
            .WithTags("Account")
            .WithDisplayName("Sign in")
            .WithSummary("Signs the user into the application")
            .AllowAnonymous();

        app.MapGet("signout", AccountEndpoints.SignOut)
            .WithTags("Account")
            .WithDisplayName("Sign out")
            .WithSummary("Signs the user out of the application.")
            .AllowAnonymous();

        app.MapGet("MicrosoftIdentity/Account/Challenge", AccountEndpoints.Challenge)
            .WithTags("Account")
            .WithDisplayName("Challenges the user.")
            .WithSummary("Challenge generating a redirect to Azure AD to sign in the user.")
            .AllowAnonymous();

        return app;
    }
}
