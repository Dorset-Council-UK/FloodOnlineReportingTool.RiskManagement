using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FloodOnlineReportingTool.RiskManagement.Options;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.AspNetCore.Builder;

internal static class WebApplicationBuilderExtension
{
    private static ClientCertificateCredential CreateClientCertificateCredential(KeyVaultOptions keyVaultOptions)
    {
        ArgumentNullException.ThrowIfNull(keyVaultOptions);

        if (string.IsNullOrWhiteSpace(keyVaultOptions.AzureAd.DirectoryId)
            || string.IsNullOrWhiteSpace(keyVaultOptions.AzureAd.ApplicationId)
            || string.IsNullOrWhiteSpace(keyVaultOptions.AzureAd.CertificateThumbprint))
        {
            throw new InvalidOperationException("KeyVault:AzureAd DirectoryId, ApplicationId and CertificateThumbprint must be configured for deployed environments.");
        }

        using var x509Store = new X509Store(StoreLocation.LocalMachine);
        x509Store.Open(OpenFlags.ReadOnly);

        var x509Certificate = x509Store.Certificates
            .Find(X509FindType.FindByThumbprint, keyVaultOptions.AzureAd.CertificateThumbprint, validOnly: false)
            .OfType<X509Certificate2>()
            .SingleOrDefault()
            ?? throw new InvalidOperationException("Unable to load the X509 certificate for KeyVault access.");

        return new ClientCertificateCredential(
            keyVaultOptions.AzureAd.DirectoryId,
            keyVaultOptions.AzureAd.ApplicationId,
            x509Certificate);
    }

    private static void TestKeyVaults(KeyVaultOptions keyVaultOptions, TokenCredential credential)
    {
        ArgumentNullException.ThrowIfNull(keyVaultOptions);
        ArgumentNullException.ThrowIfNull(credential);

        try
        {
            var sharedClient = new SecretClient(new Uri($"https://{keyVaultOptions.SharedVault}.vault.azure.net/"), credential);
            _ = sharedClient.GetSecret("HealthCheckSecret").Value;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Shared Key Vault access failed", ex);
        }

        try
        {
            var projectClient = new SecretClient(new Uri($"https://{keyVaultOptions.Name}.vault.azure.net/"), credential);
            _ = projectClient.GetSecret("HealthCheckSecret").Value;
        }
        catch (RequestFailedException ex)
        {
            throw new InvalidOperationException("Project Key Vault access failed", ex);
        }
    }

    extension(WebApplicationBuilder builder)
    {
        /// <summary>
        /// Set up Azure Key Vault if a KeyVault name is provided in the configuration
        /// </summary>
        internal WebApplicationBuilder AddFORTAzureKeyVault()
        {
            var section = builder.Configuration
                .GetSection(KeyVaultOptions.SectionName);

            var keyVaultOptions = section.Get<KeyVaultOptions>();

            if (string.IsNullOrWhiteSpace(keyVaultOptions?.SharedVault) || string.IsNullOrWhiteSpace(keyVaultOptions?.Name))
            {
                return builder;
            }

            TokenCredential credential = keyVaultOptions.AzureAd.UseDefaultCredential
                ? new DefaultAzureCredential()
                : CreateClientCertificateCredential(keyVaultOptions);

            TestKeyVaults(keyVaultOptions, credential);

            //This KeyVault has options shared over multiple projects
            builder.Configuration.AddAzureKeyVault(
                new Uri($"https://{keyVaultOptions.SharedVault}.vault.azure.net/"),
                credential);

            //This KeyVault is specific to this project
            builder.Configuration.AddAzureKeyVault(
                new Uri($"https://{keyVaultOptions.Name}.vault.azure.net/"),
                credential);

            return builder;
        }

        internal (RiskManagementOptions RiskManagement, ProxyOptions? Proxy) AddRiskManagementOptions()
        {

            var riskManagementOptions = builder.AddProjectOptions();
            var proxyOptions = builder.AddProxyOptions();
            builder.AddMiniMapOptions();
            builder.AddAzureBlobStorageOptions();

            return (riskManagementOptions, proxyOptions);
        }
    }
}
