using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FORT.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

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
        var proxySection = builder.Configuration
            .GetSection(ProxyOptions.SectionName);
        var ProxyOptionsObject = proxySection.Get<ProxyOptions>();

        if (ProxyOptionsObject != null && ProxyOptionsObject.UseForwardedHeadersMiddleware)
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                string[] currentProxies = ProxyOptionsObject.KnownProxies;
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

            //    var section = builder.Configuration
            //    .GetSection("FORT")
            //    .GetSection(KeyVaultOptions.SectionName);
            //    var keyVaultOptions = section.Get<KeyVaultOptions>();

            //    using var x509Store = new X509Store(StoreLocation.LocalMachine);
            //    x509Store.Open(OpenFlags.ReadOnly);

            //    try
            //    {
            //        var x509Certificate = x509Store.Certificates
            //        .Find(X509FindType.FindByThumbprint, keyVaultOptions.AzureAd.CertificateThumbprint, validOnly: false)
            //        .OfType<X509Certificate2>()
            //        .Single();

            //        var credential = new ClientCertificateCredential(
            //            keyVaultOptions.AzureAd.DirectoryId,
            //            keyVaultOptions.AzureAd.ApplicationId,
            //            x509Certificate);

            //        var sharedClient = new SecretClient(new Uri($"https://{keyVaultOptions.SharedVault}.vault.azure.net/"), credential);

            //        KeyVaultSecret proxySecret = sharedClient.GetSecret("ProxySettings--KnownProxies");
            //        string proxyString = proxySecret.Value;
            //        string[]? proxyOptions = JsonSerializer.Deserialize<string[]>(proxyString);

            //        if (proxyOptions is not null)
            //        {
            //            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            //            {
            //                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            //                string[] currentProxies = proxyOptions;
            //                if (currentProxies is not null)
            //                {
            //                    foreach (var proxy in currentProxies)
            //                    {
            //                        if (IPAddress.TryParse(proxy, out var ipAddress))
            //                        {
            //                            options.KnownProxies.Add(ipAddress);
            //                        }
            //                    }
            //                }
            //            });
            //        }
            //        else
            //        {
            //            throw new InvalidOperationException("Known proxies are missing");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        throw new InvalidOperationException("Shared Key Vault access failed", ex);
            //    }
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

        using var x509Store = new X509Store(StoreLocation.LocalMachine);
        x509Store.Open(OpenFlags.ReadOnly);
 
        if (string.IsNullOrWhiteSpace(keyVaultOptions?.SharedVault) || string.IsNullOrWhiteSpace(keyVaultOptions?.Name))
        {
            return builder;
        }

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

        TestKeyVaults(keyVaultOptions, x509Certificate);

        return builder;
    }

    private static void TestKeyVaults(KeyVaultOptions keyVaultOptions, X509Certificate2 x509Certificate)
    {
        //Generate a credential and use it to test for the health check settings in each KeyVault
        var credential = new ClientCertificateCredential(
            keyVaultOptions.AzureAd.DirectoryId,
            keyVaultOptions.AzureAd.ApplicationId,
            x509Certificate);

        try
        {
            var sharedClient = new SecretClient(new Uri($"https://{keyVaultOptions.SharedVault}.vault.azure.net/"), credential);
            var sharedHealthCheck = sharedClient.GetSecret("HealthCheckSecret").Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Shared Key Vault access failed", ex);
        }

        try
        {
            var projectClient = new SecretClient(new Uri($"https://{keyVaultOptions.Name}.vault.azure.net/"), credential);
            var projectHealthCheck = projectClient.GetSecret("HealthCheckSecret").Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Project Key Vault access failed", ex);
        }
    }
}