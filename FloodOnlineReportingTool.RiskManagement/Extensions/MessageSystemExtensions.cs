using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Topics;
using FloodOnlineReportingTool.RiskManagement.Consumers;
using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
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
    public static IHostApplicationBuilder AddMessageSystem(this IHostApplicationBuilder builder, MessagingOptions messagingOptions)
    {
        if (!messagingOptions.Enabled)
        {
            return builder;
        }

        builder.Services.AddMassTransit(o =>
        {
            o.SetKebabCaseEndpointNameFormatter();

            o.AddConsumer<FloodReportConsumers>();

            // Transactional Outbox Configuration https://masstransit.io/documentation/configuration/middleware/outbox
            o.AddEntityFrameworkOutbox<RiskManagementDbContext>(config =>
            {
                config.UsePostgres();
                config.UseBusOutbox();
            });

            // Using Service Bus Dead-letter Queues https://masstransit.io/documentation/configuration/transports/azure-service-bus#using-service-bus-dead-letter-queues
            o.AddConfigureEndpointsCallback((_, config) =>
            {
                if (config is IServiceBusReceiveEndpointConfigurator sb)
                {
                    sb.ConfigureDeadLetterQueueDeadLetterTransport();
                    sb.ConfigureDeadLetterQueueErrorTransport();
                }
            });

            // Azure Service Bus Configuration https://masstransit.io/documentation/configuration/transports/azure-service-bus
            // Azure Service Bus Transport https://masstransit.io/documentation/transports/azure-service-bus
            o.UsingAzureServiceBus((context, config) =>
            {
                if (messagingOptions.ConnectionString!.Contains("Endpoint=", StringComparison.OrdinalIgnoreCase))
                {
                    config.Host(messagingOptions.ConnectionString);
                }
                else
                {
                    // Using Azure Managed Identity https://masstransit.io/documentation/configuration/transports/azure-service-bus#using-azure-managed-identity
                    config.Host(new Uri(messagingOptions.ConnectionString));
                }

                config.Message<FloodReportCreated>(m =>
                {
                    m.SetEntityName(TopicNames.FloodReportCreated);
                });

                // Subscriptions https://masstransit.io/documentation/transports/azure-service-bus#subscriptions
                config.SubscriptionEndpoint<FloodReportCreated>("floodreport-riskmanagementPW", e =>
                {
                    e.ConfigureConsumer<FloodReportConsumers>(context);
                });

                // Finalise all remaining endpoint topology
                config.ConfigureEndpoints(context);
            });
        });

        return builder;
    }
}
