using FloodOnlineReportingTool.RiskManagement.Settings;
using MassTransit;

namespace Microsoft.AspNetCore.Builder;

internal static class MessageSystemExtensions
{
    /// <summary>
    /// Add the message system.
    /// </summary>
    public static IServiceCollection AddMessageSystem(this IServiceCollection services, MessagingSettings messagingSettings)
    {
        if (messagingSettings.Enabled)
        {
            services.AddMassTransit(o =>
            {
                var assembly = typeof(MessageSystemExtensions).Assembly;

                o.SetKebabCaseEndpointNameFormatter();
                o.AddServiceBusMessageScheduler();
                o.AddConsumers(assembly);

                o.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(messagingSettings.ConnectionString);
                    cfg.UseServiceBusMessageScheduler();
                    cfg.ConfigureEndpoints(context);
                });
            });
        }

        return services;
    }
}
