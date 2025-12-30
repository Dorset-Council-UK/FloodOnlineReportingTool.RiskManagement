using FloodOnlineReportingTool.RiskManagement.Options;
using MassTransit;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class MessageSystemExtensions
{
    /// <summary>
    /// Add the message system.
    /// </summary>
    internal static TBuilder AddMessageSystem<TBuilder>(this TBuilder builder, MessagingOptions messagingOptions) where TBuilder : IHostApplicationBuilder
    {
        if (!messagingOptions.Enabled)
        {
            return builder;
        }

        builder.Services.AddMassTransit(o =>
        {
            o.SetKebabCaseEndpointNameFormatter();
            o.AddConsumers(typeof(MessageSystemExtensions).Assembly);

            o.UsingAzureServiceBus((context, config) =>
            {
                if (messagingOptions.ConnectionString.Contains("Endpoint=", StringComparison.OrdinalIgnoreCase))
                {
                    config.Host(messagingOptions.ConnectionString);
                }
                else
                {
                    // Using Azure Managed Identity https://masstransit.io/documentation/configuration/transports/azure-service-bus#using-azure-managed-identity
                    config.Host(new Uri(messagingOptions.ConnectionString));
                }

                config.ConfigureEndpoints(context);
            });
        });

        return builder;
    }
}
