namespace FloodOnlineReportingTool.RiskManagement.Models.Order;

internal static class ReportPages
{
    private static ReadOnlySpan<char> BaseUrl => "report";

    public static readonly PageInfo Details = new(BaseUrl, "/details", "View a flood report");
}