namespace FloodOnlineReportingTool.RiskManagement.Models.Export;

public sealed class FloodReportExportDto
{
    public string Reference { get; init; } = string.Empty;
    public string ReportedOn { get; init; } = string.Empty;
    public string RecordStatus { get; init; } = string.Empty;
    public IReadOnlyList<FloodReportCoordinateExportDto> GeometryCoordinates { get; init; } = [];
}

public sealed class FloodReportCoordinateExportDto
{
    public double X { get; init; }
    public double Y { get; init; }
}
