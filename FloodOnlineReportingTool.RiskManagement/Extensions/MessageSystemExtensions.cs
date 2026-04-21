using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Topics;
using FloodOnlineReportingTool.RiskManagement.Consumers;
using FloodOnlineReportingTool.RiskManagement.Options;
using MassTransit;

namespace Microsoft.AspNetCore.Builder;

internal static class MessageSystemExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Add the message system.
        /// </summary>
        internal IHostApplicationBuilder AddMessageSystem(MessagingOptions messagingOptions)
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

                    config.Message<FloodReportSourceCreated>(m =>
                    {
                        m.SetEntityName(TopicNames.FloodSourceCreated);
                    });

                    // Subscriptions https://masstransit.io/documentation/transports/azure-service-bus#subscriptions
                    config.SubscriptionEndpoint<FloodReportSourceCreated>("floodreport-riskmanagement", o =>
                    {
                        o.ConfigureConsumer<FloodReportSourceCreatedConsumer>(context);
                    });
                });
            });

            return builder;
        }
    }
}
