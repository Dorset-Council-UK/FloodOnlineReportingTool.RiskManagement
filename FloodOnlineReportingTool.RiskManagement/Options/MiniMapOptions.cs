namespace FloodOnlineReportingTool.RiskManagement.Options;

/// <summary>
/// Configuration options for the MiniMap Blazor component.
/// </summary>
public class MiniMapOptions
{
    public const string SectionName = "MiniMap";

    /// <summary>
    /// OS Data Hub API key for the Ordnance Survey Vector Tile Service.
    /// </summary>
    public string OsApiKey { get; init; } = string.Empty;

    /// <summary>
    /// OS Data Hub licence number used in map attribution.
    /// </summary>
    public string OsLicenceNumber { get; init; } = string.Empty;

    /// <summary>
    /// Default map bounds in WGS84 decimal degrees [west, south, east, north] format.
    /// Defaults to approximate bounds of the South of England.
    /// </summary>
    public double[] DefaultBounds { get; init; } = [-3.5, 50.4, -1.0, 51.2];
}
