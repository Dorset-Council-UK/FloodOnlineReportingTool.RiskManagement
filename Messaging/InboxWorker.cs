using Azure.Messaging.ServiceBus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Topics;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using System.Diagnostics;
using System.Text.Json;

namespace Messaging;

public sealed class InboxWorker(
    ILogger<InboxWorker> logger,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ServiceBusClient serviceBusClient
) : BackgroundService
{
    public const string ActivitySourceName = "InboxWorker";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);
    private readonly JsonSerializerOptions _jsonOptions = JsonSerializerOptions.Web;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        string subscriptionName = configuration["Inbox:SubscriptionName"]
            ?? throw new InvalidOperationException("Missing configuration setting: 'Inbox:SubscriptionName' is required.");

        string? suffix = configuration["Messaging:TopicSuffix"];
        string topicName = string.IsNullOrWhiteSpace(suffix) ? TopicNames.FloodReportCreated : $"{TopicNames.FloodReportCreated}{suffix}";

        logger.LogInformation("Service bus processor starting...");

        await using var processor = serviceBusClient.CreateProcessor(topicName, subscriptionName, options: new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 2,
        });

        processor.ProcessMessageAsync += HandleMessageAsync;
        processor.ProcessErrorAsync += HandleErrorAsync;

        await processor.StartProcessingAsync(cancellationToken);
        logger.LogInformation("Service bus processor started");

        try
        {
            // This is not an infinite loop. Wait until cancelled. Low CPU usage while waiting.
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // expected on shutdown
        }
        finally
        {
            await processor.StopProcessingAsync(CancellationToken.None);

            // Detach event handlers to prevent them from being called after disposal
            processor.ProcessMessageAsync -= HandleMessageAsync;
            processor.ProcessErrorAsync -= HandleErrorAsync;
        }

        logger.LogInformation("Service bus processor stopped");
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        using var activity = _activitySource.StartActivity("Processing inbox message", ActivityKind.Consumer);

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
            await using var scope = scopeFactory.CreateAsyncScope();
            IActionLogRepository actionLogRepository = scope.ServiceProvider.GetRequiredService<IActionLogRepository>();
            await actionLogRepository.CreateActionLog(message, args.CancellationToken);

            await args.CompleteMessageAsync(args.Message, args.CancellationToken);

            logger.LogInformation("Processed inbox message {MessageId}", args.Message.MessageId);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Deserialization error for message {MessageId}", args.Message.MessageId);
            // Decided to just dead-letter but we may want to consider doing additional steps here. 
            // Could indicate a bad integration from a third party or something similar so we might want to 
            // notify a user. 
            await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.InnerException?.Message ?? "", cancellationToken: args.CancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing message {MessageId}", args.Message.MessageId);
            // Dead-letter the message here
            await args.DeadLetterMessageAsync(args.Message, ex.Message, ex.InnerException?.Message ?? "", cancellationToken: args.CancellationToken);
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Inbox worker error on Entity path: {EntityPath} Source: {ErrorSource}", args.EntityPath, args.ErrorSource);
        // Should this be dead lettering?
        return Task.CompletedTask;
    }
}
