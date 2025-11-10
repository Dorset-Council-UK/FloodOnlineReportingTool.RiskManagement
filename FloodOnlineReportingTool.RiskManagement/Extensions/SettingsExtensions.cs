using FloodOnlineReportingTool.RiskManagement.Exceptions;
using FloodOnlineReportingTool.RiskManagement.Settings;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Text.Json;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class SettingsExtensions
{
    internal static RiskManagementSettings AddSettings<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetRequiredSection(RiskManagementSettings.SectionName);
        builder.Services.Configure<RiskManagementSettings>(section);
        var settings = section.Get<RiskManagementSettings>()
            ?? throw new ConfigurationMissingException($"Configuration section '{RiskManagementSettings.SectionName}' is not properly defined.");
        return settings;
    }

    internal static MessagingSettings AddMessagingSettings<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetRequiredSection(MessagingSettings.SectionName);
        builder.Services.Configure<MessagingSettings>(section);
        var settings = section.Get<MessagingSettings>()
            ?? throw new ConfigurationMissingException($"Configuration section '{MessagingSettings.SectionName}' is not properly defined.");
        return settings;
    }

    /// <summary>
    /// Configures proxy settings for the application and optionally sets up forwarded headers middleware.
    /// </summary>
    /// <remarks>
    ///     <para>This method retrieves proxy settings from the application's configuration using the section name defined in <see cref="ProxySettings.SectionName"/>.</para>
    ///     <para>
    ///         If the <see cref="ProxySettings.UseForwardedHeadersMiddleware"/> property is set to <see langword="true"/>,
    ///         the method configures the <see cref="ForwardedHeadersOptions"/> to handle forwarded headers, including adding known proxies if specified.
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     A <see cref="ProxySettings"/> instance containing the configured proxy settings,
    ///     or <see langword="null"/> if no proxy settings are defined in the configuration.
    /// </returns>
    internal static ProxySettings? AddProxySettings<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetSection(ProxySettings.SectionName);
        builder.Services.Configure<ProxySettings>(section);
        var settings = section.Get<ProxySettings>();

        if (settings is null)
        {
            return null;
        }

        if (!settings.UseForwardedHeadersMiddleware)
        {
            return settings;
        }

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            //KeyVault secrets only map to string rather than string[] so we need to cast it here
            var knownProxies = JsonSerializer.Deserialize<string[]>(settings.KnownProxies);
            if (knownProxies is not null)
            {
                foreach (var proxy in knownProxies)
                {
                    if (IPAddress.TryParse(proxy, out var ipAddress))
                    {
                        options.KnownProxies.Add(ipAddress);
                    }
                }
            }
        });

        return settings;
    }
}
