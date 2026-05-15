namespace FloodOnlineReportingTool.RiskManagement.Options;

public class MessagingOptions
{
    public const string SectionName = "Messaging";

    public required bool Enabled { get; init; }
    public required string ConnectionString { get; init; }
    public string? SubscriptionNameOverride { get; init; }
}
