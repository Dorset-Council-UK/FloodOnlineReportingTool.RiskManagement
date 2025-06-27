namespace FloodOnlineReportingTool.RiskManagement.Settings;

public class MessagingSettings
{
    public const string SectionName = "Messaging";

    public required bool Enabled { get; init; }
    public required string ConnectionString { get; init; }
    public required string QueueName { get; init; }
    public required string TopicName { get; init; }
    public required string SubscriptionName { get; init; }
}
