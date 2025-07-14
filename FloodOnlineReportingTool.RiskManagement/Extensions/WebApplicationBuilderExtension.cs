using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.AspNetCore.Builder;

internal static class WebApplicationBuilderExtension 
{
    /// <summary>
    /// Add resilience to the HttpClient
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    internal static WebApplicationBuilder AddResilience(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();
        });

        return builder;
    }

    /// <summary>
    /// Add all the Risk Management project options
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    internal static WebApplicationBuilder AddFORTOptions(this WebApplicationBuilder builder)
    {

        builder.Services
            .Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;

                // Add your firewall's IP address here
                options.KnownProxies.Add(System.Net.IPAddress.Parse("172.23.40.19"));
                options.KnownProxies.Add(System.Net.IPAddress.Parse("172.23.40.15"));
                options.KnownProxies.Add(System.Net.IPAddress.Parse("172.23.40.25"));
            });

        return builder;
    }

    /// <summary>
    /// Add the Microsoft Identity Web App authentication
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    internal static WebApplicationBuilder AddFORTAuthentication(this WebApplicationBuilder builder)
    {
        const string b2cClientName = "B2CResilient";

        var azureAdSection = builder.Configuration
            .GetSection("FORT")
            .GetSection("AzureAd");

        // Register a named HttpClient for B2C with resilience
        builder.Services
            .AddHttpClient(b2cClientName)
            .AddStandardResilienceHandler();

        // Add microsoft identity web app authentication
        builder.Services
            .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(azureAdSection);
        builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();
        builder.Services.AddRazorPages();

        // Configure OpenIdConnectOptions to use our resilient HttpClient
        builder.Services
            .AddOptions<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme)
            .Configure<IHttpClientFactory>((options, httpClientFactory) =>
            {
                options.Backchannel = httpClientFactory.CreateClient(b2cClientName);
                options.SignedOutRedirectUri = "/account/signedout";
                options.AccessDeniedPath = "/account/accessdenied";
            });

        builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.AccessDeniedPath = "/account/accessdenied";
        });

        return builder;
    }
}
