namespace FloodOnlineReportingTool.RiskManagement.Options
{
    public class NetworkingOptions
    {
        public const string SectionName = "Networking";
        public bool UseForwardedHeadersMiddleware { get; init; }
        public string[] KnownProxies { get; init; } = [];
    }
}
