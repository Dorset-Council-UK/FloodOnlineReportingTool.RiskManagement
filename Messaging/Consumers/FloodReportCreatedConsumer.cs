using Azure.Messaging.ServiceBus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Topics;
using FloodOnlineReportingTool.Database.Repositories;
using System.Text.Json;

namespace Messaging.Consumers;

public sealed class FloodReportCreatedConsumer(
    ILogger<FloodReportCreatedConsumer> logger,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory
    ) : IInboxConsumer
{
    public string ContractName => nameof(FloodReportCreated);

    public string TopicName => $"{TopicNames.FloodReportCreated}{configuration["Messaging:TopicSuffix"]?.Trim()}";

    public ServiceBusProcessorOptions ProcessorOptions => new()
    {
        AutoCompleteMessages = false,
        MaxConcurrentCalls = 2,
    };

    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    public async Task HandleMessage(ProcessMessageEventArgs args)
    {
        logger.LogInformation("Consuming contract: {Contract}", nameof(FloodReportCreated));

        try
        {
            var message = JsonSerializer.Deserialize<FloodReportCreated>(args.Message.Body, _jsonOptions);
            if (message is null)
            {
                logger.LogError("Reading {Contract} {MessageId} failed", ContractName, args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "Message could not be read", cancellationToken: args.CancellationToken);
                return;
            }

            // InboxWorker and Consumers are singletons, so we have to create a scope to our service
            await using var scope = scopeFactory.CreateAsyncScope();
            IActionLogRepository actionLogRepository = scope.ServiceProvider.GetRequiredService<IActionLogRepository>();
            await actionLogRepository.CreateActionLog(message, args.CancellationToken);

            await args.CompleteMessageAsync(args.Message, args.CancellationToken);

            logger.LogInformation("Processed {Contract} {MessageId}", ContractName, args.Message.MessageId);
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

    public Task HandleError(ProcessErrorEventArgs args)
    {
        logger.LogCritical(args.Exception, "{Contract} error on Entity path: {EntityPath} Source: {ErrorSource}", ContractName, args.EntityPath, args.ErrorSource);
        return Task.CompletedTask;
    }
}
