namespace FloodOnlineReportingTool.RiskManagement.Settings;

public class RiskManagementSettings
{
    public const string SectionName = "RiskManagement";
    public const string ReportStatusApi = "ReportStatusApi";

    public required string PathBase { get; init; }
}
