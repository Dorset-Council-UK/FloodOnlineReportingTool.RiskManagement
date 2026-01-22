using FloodOnlineReportingTool.RiskManagement.Authorization;
using FloodOnlineReportingTool.RiskManagement.Endpoints.Account;
using FloodOnlineReportingTool.RiskManagement.Options;
using FloodOnlineReportingTool.RiskManagement.Settings;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

namespace Microsoft.AspNetCore.Builder;

internal static class AuthenticationExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Configures authentication, authorization and policies.
        /// </summary>
        internal IHostApplicationBuilder AddAuthentication()
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

            builder.Services.AddResilientHttpClients();

            // Add Blazor cascading authentication state
            builder.Services.AddCascadingAuthenticationState();

            // Setup Authorization
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
                .SetFallbackPolicy(policy: null); // Anonymous access allowed unless [Authorize] is used

            return builder;
        }
    }

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Configures resilient HttpClients for default and OpenIdConnect backchannel.
        /// </summary>
        internal IServiceCollection AddResilientHttpClients()
        {
            // Default HttpClient resilience
            services
                .AddHttpClient(Options.DefaultName)
                .AddStandardResilienceHandler();

            // OpenIdConnect backchannel resilience
            const string clientName = "OpenIdConnectResilient";
            services
                .AddHttpClient(clientName)
                .AddStandardResilienceHandler();

            services
                .AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
                .Configure<IHttpClientFactory>((options, httpClientFactory) =>
                {
                    options.Backchannel = httpClientFactory.CreateClient(clientName);
                });

            return services;
        }
    }

    extension(WebApplication app)
    {
        /// <summary>
        /// Configures authentication-related HTTP endpoints for the application, including sign in, sign out, and Microsoft Identity challenge routes.
        /// </summary>
        /// <returns>The <see cref="WebApplication"/> instance with authentication endpoints mapped.</returns>
        internal WebApplication MapAuthenticationEndpoints()
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
}
