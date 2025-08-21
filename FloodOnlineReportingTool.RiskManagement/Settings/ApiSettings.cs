namespace FloodOnlineReportingTool.RiskManagement.Settings;

public class ApiSettings
{
    public const string SectionName = "Api";

    public required string BaseUrl { get; init; } = "https://localhost:7065/";
    public int TimeoutSeconds { get; init; } = 30;
    public string UserAgent { get; init; } = "FloodRiskManagement/1.0";
}