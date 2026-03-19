namespace FloodOnlineReportingTool.RiskManagement.Models.Order;

internal static class EventPages
{
    private static ReadOnlySpan<char> BaseUrl => "events";

    public static readonly PageInfo Home = new(BaseUrl, "View flood event");
    public static readonly PageInfo Create = new(BaseUrl, "/create", "Create a flood event");
    public static readonly PageInfo Change = new(BaseUrl, "/change", "Update a flood event");
    public static readonly PageInfo Delete = new(BaseUrl, "/delete", "Archive / delete a flood event");
}
