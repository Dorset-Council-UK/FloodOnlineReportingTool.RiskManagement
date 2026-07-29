namespace FloodOnlineReportingTool.RiskManagement.Models.Order;

internal static class ReportPages
{
    private static ReadOnlySpan<char> BaseUrl => "report";

    public static readonly PageInfo Details = new(BaseUrl, "/details", "View report");
    public static readonly PageInfo Attachments = new(BaseUrl, "/attachments", "View report attachments");
    public static readonly PageInfo Upload = new(BaseUrl, "/upload", "Upload attachments");
    public static readonly PageInfo Comments = new(BaseUrl, "/comments", "View report comments");
}