using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Tokens;
using System.Configuration;
using static System.Net.Mime.MediaTypeNames;

namespace Microsoft.AspNetCore.Builder;

internal static class WebApplicationBuilderExtension
{
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
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

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

        // Add services
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("FORT").GetSection("AzureAd"));

        builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            })
            .AddCascadingAuthenticationState();

        builder.Services.AddControllersWithViews()
            .AddMicrosoftIdentityUI();

        return builder;
    }
}