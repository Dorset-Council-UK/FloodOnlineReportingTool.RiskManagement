using Azure.Messaging.ServiceBus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using System.Text.Json;

namespace Messaging.Consumers;

public sealed class FloodReportCreatedConsumer(
    ILogger<FloodReportCreatedConsumer> logger,
    IServiceScopeFactory scopeFactory)
{

    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    public async Task HandleMessage(ProcessMessageEventArgs args)
    {
        logger.LogInformation("Consuming contract: {Contract}", nameof(FloodReportCreated));

        try
        {
            var message = JsonSerializer.Deserialize<FloodReportCreated>(args.Message.Body, _jsonOptions);
            if (message is null)
            {
                logger.LogError("Reading inbox message {MessageId} failed", args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "Deserialization returned null", cancellationToken: args.CancellationToken);
                return;
            }

            // InboxWorker / BackgroundService is a singleton, so we have to create a scope to our service
            await using var scope = _scopeFactory.CreateAsyncScope();
            IActionLogRepository actionLogRepository = scope.ServiceProvider.GetRequiredService<IActionLogRepository>();
            await actionLogRepository.CreateActionLog(message, args.CancellationToken);

            await args.CompleteMessageAsync(args.Message, args.CancellationToken);

            logger.LogInformation("Processed inbox message {MessageId}", args.Message.MessageId);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Deserialization error for message {MessageId}", args.Message.MessageId);
            await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.InnerException?.Message ?? "", cancellationToken: args.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message {MessageId}", args.Message.MessageId);
            await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.InnerException?.Message ?? "", cancellationToken: args.CancellationToken);
        }
    }
}
