using FloodOnlineReportingTool.Contracts;
using MassTransit;

namespace FloodOnlineReportingTool.RiskManagement.Consumers;

internal class FloodReportCreatedConsumer(ILogger<FloodReportCreatedConsumer> logger) : IConsumer<FloodReportSourceCreated>
{
    public Task Consume(ConsumeContext<FloodReportSourceCreated> context)
    {
        // We record the action of new as being received.
        logger.LogInformation("{Consumer}: {Message}", nameof(FloodReportCreatedConsumer), context.Message.ToString());
        return Task.CompletedTask;
    }
}
