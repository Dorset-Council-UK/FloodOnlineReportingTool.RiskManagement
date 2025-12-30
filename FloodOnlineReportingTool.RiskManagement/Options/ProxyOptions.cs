namespace FloodOnlineReportingTool.RiskManagement.Options;

public class ProxyOptions
{
    public const string SectionName = "ProxyOptions";

    public bool UseForwardedHeadersMiddleware { get; set; }
    public string KnownProxies { get; init; } = "";
    public string AllowedHosts { get; set; } = "";
    public bool UseHttpsRedirection { get; set; }
}
