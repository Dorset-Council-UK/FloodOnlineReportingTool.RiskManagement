namespace FloodOnlineReportingTool.RiskManagement.Models.Search
{
    public enum ApiFloodReferenceSearchType
    {
        /// <summary>Search against the flood report's own generated reference (e.g. "A1B2C3D4").</summary>
        Report,

        /// <summary>Search against the originating source system's reference.</summary>
        Source,
    }
}
