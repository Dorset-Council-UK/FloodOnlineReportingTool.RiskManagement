using FloodOnlineReportingTool.RiskManagement.Exceptions;
using FloodOnlineReportingTool.RiskManagement.Options;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Text.Json;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class OptionsExtensions
{

    internal static RiskManagementOptions AddProjectOptions<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetRequiredSection(RiskManagementOptions.SectionName);
        builder.Services.Configure<RiskManagementOptions>(section);
        var options = section.Get<RiskManagementOptions>()
            ?? throw new ConfigurationMissingException($"Configuration section '{RiskManagementOptions.SectionName}' is not properly defined.");
        return options;
    }

    internal static void AddMiniMapOptions<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetSection(MiniMapOptions.SectionName);
        builder.Services.Configure<MiniMapOptions>(section);
    }

    internal static MessagingOptions AddMessagingOptions<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetRequiredSection(MessagingOptions.SectionName);
        builder.Services.Configure<MessagingOptions>(section);
        var options = section.Get<MessagingOptions>()
            ?? throw new ConfigurationMissingException($"Configuration section '{MessagingOptions.SectionName}' is not properly defined.");
        return options;
    }

    /// <summary>
    /// Configures proxy options for the application and optionally sets up forwarded headers middleware.
    /// </summary>
    /// <remarks>
    ///     <para>This method retrieves proxy options from the application's configuration using the section name defined in <see cref="ProxyOptions.SectionName"/>.</para>
    ///     <para>
    ///         If the <see cref="ProxyOptions.UseForwardedHeadersMiddleware"/> property is set to <see langword="true"/>,
    ///         the method configures the <see cref="ForwardedHeadersOptions"/> to handle forwarded headers, including adding known proxies if specified.
    ///     </para>
    /// </remarks>
    /// <returns>
    ///     A <see cref="ProxyOptions"/> instance containing the configured proxy options,
    ///     or <see langword="null"/> if no proxy options are defined in the configuration.
    /// </returns>
    internal static ProxyOptions? AddProxyOptions<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var section = builder.Configuration.GetSection(ProxyOptions.SectionName);
        builder.Services.Configure<ProxyOptions>(section);
        var proxyOptions = section.Get<ProxyOptions>();

        if (proxyOptions is null)
        {
            return null;
        }

        if (!proxyOptions.UseForwardedHeadersMiddleware)
        {
            return proxyOptions;
        }

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            //KeyVault secrets only map to string rather than string[] so we need to cast it here
            var knownProxies = JsonSerializer.Deserialize<string[]>(proxyOptions.KnownProxies);
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

        return proxyOptions;
    }
}
