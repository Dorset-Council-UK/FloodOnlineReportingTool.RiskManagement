namespace FloodOnlineReportingTool.RiskManagement.Models;

/// <summary>
/// Configuration for a WMS-based overlay layer to display on the MiniMap.
/// </summary>
public class WmsLayerConfig
{
    /// <summary>
    /// Unique identifier for this layer, used as the MapLibre source/layer ID.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Base URL of the WMS service endpoint (without query string parameters).
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Comma-separated list of WMS layer names to request.
    /// </summary>
    public required string Layers { get; init; }

    /// <summary>
    /// WMS image format. Defaults to image/png.
    /// </summary>
    public string Format { get; init; } = "image/png";
}
