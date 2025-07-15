using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
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

        var azureAdSection = builder.Configuration
            .GetSection("FORT")
            .GetSection("AzureAd");

        // Add microsoft identity web app authentication
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.AccessDeniedPath = "/account/accessdenied";
            })
            .AddOpenIdConnect(options =>
            {
                options.ClientId = azureAdSection["ClientId"];
                options.ClientSecret = azureAdSection["ClientSecret"];
                options.Authority = azureAdSection["Authority"] + "v2.0/";
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                options.UsePkce = true;
                options.ResponseType = "code";
                options.ResponseMode = "form_post";

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("email");
                options.Scope.Add("profile");
                options.Scope.Add("offline_access");

                options.GetClaimsFromUserInfoEndpoint = true;
                options.UseTokenLifetime = false;
                options.SaveTokens = true;
                options.RequireHttpsMetadata = true;

                options.CallbackPath = "/signin-oidc";
            }
        );
        //builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();
        builder.Services.AddRazorPages();
        builder.Services.AddAuthorization();

        builder.Services
            .AddCascadingAuthenticationState()
            .AddAuthorizationBuilder()
            .AddPolicy("AuthenticatedOnly", policy => policy.RequireAuthenticatedUser());
        return builder;
    }
}