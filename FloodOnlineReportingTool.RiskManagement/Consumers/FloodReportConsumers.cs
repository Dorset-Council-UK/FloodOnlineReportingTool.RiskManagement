using FloodOnlineReportingTool.Contracts;
using MassTransit;

namespace FloodOnlineReportingTool.RiskManagement.Consumers;

internal class FloodReportConsumers(ILogger<FloodReportConsumers> logger) 
    : IConsumer<FloodReportCreated>,
    IConsumer<FloodReportUpdated>
{
    public Task Consume(ConsumeContext<FloodReportCreated> context)
    {
        // We record a new action for the users of the system to complete.
        logger.LogInformation("Consuming contract: {Contract}", nameof(FloodReportCreated));
        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<FloodReportUpdated> context)
    {
        // We assess the change to determine if a new action is required or whether or not we are clearing an action instead.
        logger.LogInformation("Consuming contract: {Contract}", nameof(FloodReportUpdated));
        return Task.CompletedTask;
    }
}
