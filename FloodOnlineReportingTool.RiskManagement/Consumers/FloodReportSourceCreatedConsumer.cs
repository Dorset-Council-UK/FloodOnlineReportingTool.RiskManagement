using FloodOnlineReportingTool.Contracts;
using MassTransit;

namespace FloodOnlineReportingTool.RiskManagement.Consumers;

internal class FloodReportSourceCreatedConsumer(ILogger<FloodReportSourceCreatedConsumer> logger) : IConsumer<FloodReportSourceCreated>
{
    public Task Consume(ConsumeContext<FloodReportSourceCreated> context)
    {
        // We record the action of new as being received.
        logger.LogInformation("{Consumer}: {Message}", nameof(FloodReportSourceCreatedConsumer), context.Message.ToString());
        return Task.CompletedTask;
    }
}
