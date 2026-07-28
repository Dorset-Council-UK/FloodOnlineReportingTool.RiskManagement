namespace FloodOnlineReportingTool.RiskManagement.Models.Order;

internal static class ReportPages
{
    private static ReadOnlySpan<char> BaseUrl => "report";

    public static readonly PageInfo Details = new(BaseUrl, "/details", "View a flood report");
    public static readonly PageInfo Attachments = new(BaseUrl, "/attachments", "View a flood report attachment");
    public static readonly PageInfo Comments = new(BaseUrl, "/comments", "View a flood report comment");
}