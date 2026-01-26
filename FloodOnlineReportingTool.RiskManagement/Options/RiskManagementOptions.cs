namespace FloodOnlineReportingTool.RiskManagement.Options;

public class RiskManagementOptions
{
    public const string SectionName = "RiskManagement";
    public const string ReportStatusApi = "ReportStatusApi";

    public required string PathBase { get; init; }
}
