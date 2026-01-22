using FloodOnlineReportingTool.RiskManagement.Settings;
using MassTransit;

namespace Microsoft.AspNetCore.Builder;

internal static class MessageSystemExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Add the message system.
        /// </summary>
        internal IHostApplicationBuilder AddMessageSystem(MessagingSettings messagingSettings)
        {
            if (!messagingSettings.Enabled)
            {
                return builder;
            }

            builder.Services.AddMassTransit(o =>
            {
                o.SetKebabCaseEndpointNameFormatter();
                o.AddConsumers(typeof(MessageSystemExtensions).Assembly);

                o.UsingAzureServiceBus((context, config) =>
                {
                    if (messagingSettings.ConnectionString.Contains("Endpoint=", StringComparison.OrdinalIgnoreCase))
                    {
                        config.Host(messagingSettings.ConnectionString);
                    }
                    else
                    {
                        // Using Azure Managed Identity https://masstransit.io/documentation/configuration/transports/azure-service-bus#using-azure-managed-identity
                        config.Host(new Uri(messagingSettings.ConnectionString));
                    }

                    config.ConfigureEndpoints(context);
                });
            });

            return builder;
        }
    }
}
