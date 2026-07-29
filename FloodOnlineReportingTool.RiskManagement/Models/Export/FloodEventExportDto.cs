using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Export;

public sealed class FloodEventExportDto
{
    public string Reference { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public MultiPolygon Geom { get; init; } = null!;
}
