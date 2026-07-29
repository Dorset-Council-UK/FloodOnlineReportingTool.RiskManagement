using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

public record EventOverviewViewModel 
{
    public string SearchReference { get; init; } = string.Empty;
    public string SearchTitle { get; init; } = string.Empty;

    public long? UPRN { get; init; } 
    public long? USRN { get; init; } 
    public MultiPolygon? Geom { get; init; } = null;

    // Common event properties (always required)
    public string RecordStatus { get; init; } = string.Empty;
    public string FloodStatus { get; init; } = string.Empty;
    public string InvestigationStatus { get; init; } = string.Empty;
    public int EstimatedVulnerable { get; init; } = 0;
    public int EstimatedUninhabitable { get; init; } = 0;
    public int EstimatedTotalExternalOnly { get; init; } = 0;
    public int EstimatedTotalInternallyFlooded { get; init; } = 0;
    public int EstimatedDisruptedUtilities { get; init; } = 0;
}
