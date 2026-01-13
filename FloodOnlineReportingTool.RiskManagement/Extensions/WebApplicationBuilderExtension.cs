using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FloodOnlineReportingTool.RiskManagement.Options;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.AspNetCore.Builder;

internal static class WebApplicationBuilderExtension
{
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

        //If we got to this point then we have KeyVaults to load. 
        var x509Certificate = x509Store.Certificates
            .Find(X509FindType.FindByThumbprint, keyVaultOptions.AzureAd.CertificateThumbprint, validOnly: false)
            .OfType<X509Certificate2>()
            .Single();

        //This KeyVault has options shared over multiple projects
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
        //Generate a credential and use it to test for the health check options in each KeyVault
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

    internal static (RiskManagementOptions RiskManagement, ProxyOptions? Proxy, MessagingOptions Messaging) AddRiskManagementOptions(this WebApplicationBuilder builder)
    {

        var riskManagementOptions = builder.AddProjectOptions();
        var proxyOptions = builder.AddProxyOptions();
        var messagingOptions = builder.AddMessagingOptions();

        return (riskManagementOptions, proxyOptions, messagingOptions);
    }
}