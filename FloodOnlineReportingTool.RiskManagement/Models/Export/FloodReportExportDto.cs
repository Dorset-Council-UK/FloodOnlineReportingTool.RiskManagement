namespace FloodOnlineReportingTool.RiskManagement.Models.Export;

public sealed class FloodReportExportDto
{
    public string Reference { get; init; } = string.Empty;
    public string ReportedOn { get; init; } = string.Empty;
    public string RecordStatus { get; init; } = string.Empty;
    public double Easting { get; init; }
    public double Northing { get; init; }
    public decimal X { get; init; }
    public decimal Y { get; init; }
}
