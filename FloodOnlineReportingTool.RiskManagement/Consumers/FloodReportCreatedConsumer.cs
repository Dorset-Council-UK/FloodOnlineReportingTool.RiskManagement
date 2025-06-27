using FloodOnlineReportingTool.Contracts;
using MassTransit;

namespace FloodOnlineReportingTool.RiskManagement.Consumers;

internal class FloodReportCreatedConsumer(ILogger<FloodReportCreatedConsumer> logger) : IConsumer<FloodReportCreated>
{
    public Task Consume(ConsumeContext<FloodReportCreated> context)
    {
        logger.LogInformation("{Consumer}: {Message}", nameof(FloodReportCreatedConsumer), context.Message.ToString());
        return Task.CompletedTask;
    }
}
