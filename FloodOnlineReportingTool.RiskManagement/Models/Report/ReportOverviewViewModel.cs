using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Report;

public record ReportOverviewViewModel
{
    public Guid Id { get; init; }
    public required DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
    public required string Reference { get; init; }
    public required Guid RecordStatusId { get; set; }
    public required Point Geom { get; init; }
    public required string LocationDescription { get; init; }
    public required decimal Latitude { get; init; }
    public required decimal Longitude { get; init; }

    public IReadOnlyCollection<ReportOverviewSourceItem> SourceRecords { get; set; } = [];
}