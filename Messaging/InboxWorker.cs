using Azure.Messaging.ServiceBus;
using FloodOnlineReportingTool.Contracts;
using FloodOnlineReportingTool.Contracts.Topics;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using Messaging.Consumers;
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

        var serviceBusOptions = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 2,
        };
        // Create all the processors for the topics we want to listen to
        var processors = new[]
        {
            serviceBusClient.CreateProcessor(
                GetTopicName(nameof(FloodReportCreated)),
                subscriptionName,
                options: serviceBusOptions),
        };

        try
        {
            // Start all processors
            foreach (var processor in processors)
            {
                processor.ProcessMessageAsync += HandleMessageAsync;
                processor.ProcessErrorAsync += HandleErrorAsync;
                await processor.StartProcessingAsync(cancellationToken);
            }
            logger.LogInformation("All processors started, waiting for cancellation...");

            // Wait for cancellation
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        finally
        {
            // Graceful shutdown
            foreach (var processor in processors)
            {
                await processor.StopProcessingAsync();
            }
            logger.LogInformation("Service bus processors stopped");
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        logger.LogError(args.Exception, "Inbox worker error on Entity path: {EntityPath} Source: {ErrorSource}", args.EntityPath, args.ErrorSource);
        // Should this be dead lettering?
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        string floodReportTopicName = GetTopicName(nameof(FloodReportCreated));

        // Determine the topic from the EntityPath and send to the handler
        using var scope = scopeFactory.CreateScope();
        // Expected is {topicName}/Subscriptions/{subscriptionName} or {queueName} for queue,
        // so we split on the first '/' to get the topic name
        string topicName = args.EntityPath.Split('/', 2, StringSplitOptions.TrimEntries)[0];
        switch (topicName)
        {
            case var t when string.Equals(t, floodReportTopicName, StringComparison.OrdinalIgnoreCase):
                var floodReportConsumer = scope.ServiceProvider.GetRequiredService<FloodReportCreatedConsumer>();
                await floodReportConsumer.HandleMessage(args);
                break;

            default:
                logger.LogError("No consumer configured for topic {TopicName} (EntityPath: {EntityPath}, MessageId: {MessageId})", topicName, args.EntityPath, args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "No consumer configured for topic", $"Missing topic name is {topicName}", cancellationToken: args.CancellationToken);
                break;
        }
    }

    private string GetTopicName(string messageType)
    {
        var topicName = messageType switch
        {
            nameof(FloodReportCreated) => TopicNames.FloodReportCreated,
            _ => throw new NotSupportedException($"Message type {messageType} is not supported."),
        };

        string? suffix = configuration["Messaging:TopicSuffix"];
        return string.IsNullOrWhiteSpace(suffix) ? topicName : $"{topicName}{suffix}";
    }
}
