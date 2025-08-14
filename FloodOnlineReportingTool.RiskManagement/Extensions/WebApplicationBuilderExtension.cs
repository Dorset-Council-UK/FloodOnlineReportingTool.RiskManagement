using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FORT.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using System.Net;
using System.Security.Cryptography.X509Certificates;

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
        var sectionKeyVault = builder.Configuration.GetSection("FORT").GetSection(KeyVaultOptions.SectionName);
        builder.Services
            .Configure<KeyVaultOptions>(sectionKeyVault);


        var sectionProxySettings = builder.Configuration.GetSection(ProxyOptions.SectionName);
        var proxyOptions = sectionProxySettings.Get<ProxyOptions>();
        if (proxyOptions is not null && proxyOptions.UseForwardedHeadersMiddleware)
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                var currentProxies = proxyOptions.KnownProxies;
                if (currentProxies is not null)
                {
                    foreach (var proxy in currentProxies)
                    {
                        if (IPAddress.TryParse(proxy, out var ipAddress))
                        {
                            options.KnownProxies.Add(ipAddress);
                        }
                    }
                }
            });
        }

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

        //builder.Services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
        //{
        //    options.Events.OnRedirectToIdentityProvider = context =>
        //    {
        //        context.ProtocolMessage.RedirectUri = "https://gi-staging.dorsetcouncil.gov.uk/floodriskmanagement/signin-oidc";
        //        return Task.CompletedTask;
        //    };

        //});

        builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            })
            .AddCascadingAuthenticationState();

        return builder;
    }

    /// <summary>
    /// Set up Azure Key Vault if a KeyVault name is provided in the configuration
    /// </summary>
    internal static WebApplicationBuilder AddFORTAzureKeyVault(this WebApplicationBuilder builder)
    {
        var section = builder.Configuration
            .GetSection("FORT")
            .GetSection(KeyVaultOptions.SectionName);

        var keyVaultOptions = section.Get<KeyVaultOptions>();

        if (string.IsNullOrWhiteSpace(keyVaultOptions?.Name))
        {
            return builder;
        }

        using var x509Store = new X509Store(StoreLocation.LocalMachine);
        x509Store.Open(OpenFlags.ReadOnly);

        var x509Certificate = x509Store.Certificates
            .Find(X509FindType.FindByThumbprint, keyVaultOptions.AzureAd.CertificateThumbprint, validOnly: false)
            .OfType<X509Certificate2>()
            .Single();

        //This KeyVault has settings shared over multiple projects
        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{keyVaultOptions.SharedVault}.vault.azure.net/"),
            new ClientCertificateCredential(keyVaultOptions.AzureAd.DirectoryId, keyVaultOptions.AzureAd.ApplicationId, x509Certificate));

        //This KeyVault is specific to this project
        builder.Configuration.AddAzureKeyVault(
            new Uri($"https://{keyVaultOptions.Name}.vault.azure.net/"),
            new ClientCertificateCredential(keyVaultOptions.AzureAd.DirectoryId, keyVaultOptions.AzureAd.ApplicationId, x509Certificate));


        // Validate access manually
        var credential = new ClientCertificateCredential(
            keyVaultOptions.AzureAd.DirectoryId,
            keyVaultOptions.AzureAd.ApplicationId,
            x509Certificate);

        var sharedClient = new SecretClient(new Uri($"https://{keyVaultOptions.SharedVault}.vault.azure.net/"), credential);
        var projectClient = new SecretClient(new Uri($"https://{keyVaultOptions.Name}.vault.azure.net/"), credential);

        //try
        //{
        //    var sharedEnumerator = sharedClient.GetPropertiesOfSecretsAsync().GetAsyncEnumerator();
        //    sharedEnumerator.MoveNextAsync().GetAwaiter().GetResult();
        //}
        //catch (Exception ex)
        //{
        //    throw new InvalidOperationException("Shared Key Vault access failed", ex);
        //}

        try
        {
            var projectEnumerator = projectClient.GetPropertiesOfSecretsAsync().GetAsyncEnumerator();
            projectEnumerator.MoveNextAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Project Key Vault access failed", ex);
        }

        return builder;
    }
}