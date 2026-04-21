namespace FloodOnlineReportingTool.RiskManagement.Models;

/// <summary>
/// Represents a single marker to be displayed on the MiniMap.
/// </summary>
/// <param name="Id">Unique identifier for the item.</param>
/// <param name="Latitude">Latitude coordinate of the marker.</param>
/// <param name="Longitude">Longitude coordinate of the marker.</param>
/// <param name="PopupHtml">HTML content to display in the marker popup.</param>
public record MapMarkerData(string Id, decimal Latitude, decimal Longitude, string PopupHtml);
