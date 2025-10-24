namespace FloodOnlineReportingTool.RiskManagement.Settings;

public class ProxySettings
{
    public const string SectionName = "ProxySettings";

    public bool UseForwardedHeadersMiddleware { get; set; }
    public string KnownProxies { get; init; } = "";
    public string AllowedHosts { get; set; } = "";
    public bool UseHttpsRedirection { get; set; }
}
