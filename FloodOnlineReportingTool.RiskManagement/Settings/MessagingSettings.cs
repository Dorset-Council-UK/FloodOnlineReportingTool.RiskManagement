namespace FloodOnlineReportingTool.RiskManagement.Settings;

public class MessagingSettings
{
    public const string SectionName = "Messaging";

    public required bool Enabled { get; init; }
    public required string ConnectionString { get; init; }
}
