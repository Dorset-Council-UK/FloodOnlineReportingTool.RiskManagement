namespace FloodOnlineReportingTool.RiskManagement.Models.Order;

internal static class SearchPages
{
    private static ReadOnlySpan<char> BaseUrl => "search";

    public static readonly PageInfo Home = new(BaseUrl, "Search");
    public static readonly PageInfo Options = new(BaseUrl, "/options", "Options");
    public static readonly PageInfo OptionsMap = new(BaseUrl, "/options-map", "Options - Search Area");
    public static readonly PageInfo Results = new(BaseUrl, "/results", "Flood reports");
    public static readonly PageInfo Property = new(BaseUrl, "/property", "Property reports");
    public static readonly PageInfo Highway = new(BaseUrl, "/highway", "Highway reports");
    public static readonly PageInfo Location = new(BaseUrl, "/location", "Location reports");
    public static readonly PageInfo Event = new(BaseUrl, "/event", "Flood event reports");
}
