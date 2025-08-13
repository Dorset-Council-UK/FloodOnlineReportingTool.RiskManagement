using FloodOnlineReportingTool.RiskManagement.Options;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

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

        var section = builder.Configuration
            .GetSection(NetworkingOptions.SectionName);
        var networkingOptions = section.Get<NetworkingOptions>();

        if (networkingOptions is not null && networkingOptions.UseForwardedHeadersMiddleware)
        {
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

                if (networkingOptions.KnownProxies is not null)
                {
                    foreach (var proxy in networkingOptions.KnownProxies)
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
}