using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using MassTransit;

namespace FloodOnlineReportingTool.RiskManagement.Consumers;

internal class FloodReportConsumers(ILogger<FloodReportConsumers> logger, IActionLogRepository actionLogRepository) 
    : IConsumer<FloodReportCreated>,
    IConsumer<FloodReportUpdated>
{
    public async Task Consume(ConsumeContext<FloodReportCreated> context)
    {
        // We record a new action for the users of the system to complete.
        logger.LogInformation("Consuming contract: {Contract}", nameof(FloodReportCreated));

        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        var result = await actionLogRepository.CreateActionLog(message, cancellationToken);
        if (result.IsSuccess)
        {
            var actionLog = result.Value;
            logger.LogInformation("Successfully created action log for report {ReportReference}", actionLog.ReportReference);
        }
        else
        {
            foreach (var error in result.Errors)
            {
                logger.LogError("Error creating action log for report {ReportReference}: {Error}", message.Reference, error);
            }
        }
    }   
    

    public async Task Consume(ConsumeContext<FloodReportUpdated> context)
    {
        // We assess the change to determine if a new action is required or whether or not we are clearing an action instead.
        logger.LogInformation("Consuming contract: {Contract}", nameof(FloodReportUpdated));

        var message = context.Message;
        var cancellationToken = context.CancellationToken;

        await Task.CompletedTask;
    }
}
