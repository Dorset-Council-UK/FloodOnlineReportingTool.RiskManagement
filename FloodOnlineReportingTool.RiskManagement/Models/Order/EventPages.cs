namespace FloodOnlineReportingTool.RiskManagement.Models.Order;

internal static class EventPages
{
    private static ReadOnlySpan<char> BaseUrl => "event";

    public static readonly PageInfo View = new(BaseUrl, "View flood event");
    public static readonly PageInfo Details = new(BaseUrl, "/details", "Set event details");
    public static readonly PageInfo Estimates = new(BaseUrl, "/estimates", "Update event estimates");
    public static readonly PageInfo Delete = new(BaseUrl, "/delete", "Archive / delete a flood event");
}
