using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FloodOnlineReportingTool.RiskManagement.Options;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.AspNetCore.Builder;

internal static class WebApplicationBuilderExtension
{
    private static void TestKeyVaults(KeyVaultOptions keyVaultOptions)
    {
        //Generate a credential and use it to test for the health check settings in each KeyVault
        //var credential = new ClientCertificateCredential(
        //    keyVaultOptions.AzureAd.DirectoryId,
        //    keyVaultOptions.AzureAd.ApplicationId,
        //    x509Certificate);

        try
        {
            var sharedClient = new SecretClient(new Uri($"https://{keyVaultOptions.SharedVaultName}.vault.azure.net/"), new DefaultAzureCredential());
            var sharedHealthCheck = sharedClient.GetSecret("HealthCheckSecret").Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Shared Key Vault access failed", ex);
        }

        try
        {
            var projectClient = new SecretClient(new Uri($"https://{keyVaultOptions.VaultName}.vault.azure.net/"), new DefaultAzureCredential());
            var projectHealthCheck = projectClient.GetSecret("HealthCheckSecret").Value;
        }
        catch (Exception ex)
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

            using var x509Store = new X509Store(StoreLocation.LocalMachine);
            x509Store.Open(OpenFlags.ReadOnly);
 
            if (string.IsNullOrWhiteSpace(keyVaultOptions?.SharedVaultName) || string.IsNullOrWhiteSpace(keyVaultOptions?.VaultName))
            {
                return builder;
            }

            //If we got to this point then we have KeyVaults to load. 
            //var x509Certificate = x509Store.Certificates
            //    .Find(X509FindType.FindByThumbprint, keyVaultOptions.AzureAd.CertificateThumbprint, validOnly: false)
            //    .OfType<X509Certificate2>()
            //    .Single();

            //new ClientCertificateCredential(keyVaultOptions.AzureAd.DirectoryId, keyVaultOptions.AzureAd.ApplicationId, x509Certificate))

            //This KeyVault has options shared over multiple projects
            builder.Configuration.AddAzureKeyVault(
                new Uri($"https://{keyVaultOptions.SharedVaultName}.vault.azure.net/"),
                new DefaultAzureCredential());

            //This KeyVault is specific to this project
            builder.Configuration.AddAzureKeyVault(
                new Uri($"https://{keyVaultOptions.VaultName}.vault.azure.net/"),
                new DefaultAzureCredential());

            TestKeyVaults(keyVaultOptions);

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