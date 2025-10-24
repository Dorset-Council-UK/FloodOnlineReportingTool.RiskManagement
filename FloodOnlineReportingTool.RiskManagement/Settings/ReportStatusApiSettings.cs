namespace FloodOnlineReportingTool.RiskManagement.Settings;

public class ReportStatusApiSettings
{
    public const string SectionName = "Api";
    public const string ClientName = "ReportStatusApiClient";

    public required string BaseUrl { get; init; } = "https://localhost:7065/";
    public int TimeoutSeconds { get; init; } = 30;
    public string UserAgent { get; init; } = "FloodRiskManagement/1.0";
}