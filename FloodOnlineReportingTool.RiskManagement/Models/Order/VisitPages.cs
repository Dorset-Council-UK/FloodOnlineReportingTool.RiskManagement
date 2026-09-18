namespace FloodOnlineReportingTool.RiskManagement.Models.Order;

internal static class VisitPages
{
    private static ReadOnlySpan<char> BaseUrl => "visit";

    public static readonly PageInfo Home = new(BaseUrl, "", "Home");
    public static readonly PageInfo Postcode = new(BaseUrl, "/postcode", "Search by postcode");
    public static readonly PageInfo Address = new(BaseUrl, "/address", "Select address");
    public static readonly PageInfo Location = new(BaseUrl, "/location", "Select location");
    public static readonly PageInfo PropertyType = new(BaseUrl, "/propertytype", "Property type");


    public static readonly PageInfo Summary = new(BaseUrl, "/summary", "Check your answers");
}