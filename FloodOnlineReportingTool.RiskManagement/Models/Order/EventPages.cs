namespace FloodOnlineReportingTool.RiskManagement.Models.Order;

internal static class EventPages
{
    private static ReadOnlySpan<char> BaseUrl => "event";

    public static readonly PageInfo View = new(BaseUrl, "View flood event");
    public static readonly PageInfo Details = new(BaseUrl, "/details", "Set event details");
    public static readonly PageInfo Estimates = new(BaseUrl, "/estimates", "Update event estimates");
    public static readonly PageInfo Delete = new(BaseUrl, "/delete", "Archive / delete a flood event");
    public static readonly PageInfo Upload = new(BaseUrl, "/upload", "Upload attachments");
    public static readonly PageInfo Attachments = new(BaseUrl, "/attachments", "View event attachments");
    public static readonly PageInfo Comments = new(BaseUrl, "/comments", "View event comments");
    public static readonly PageInfo PropertySummary = new(BaseUrl, "/propertysummary", "View event property summary");
    public static readonly PageInfo ImpactSummary = new(BaseUrl, "/comments", "View event impact summary");
}
