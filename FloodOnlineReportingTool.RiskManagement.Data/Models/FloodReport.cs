namespace FloodOnlineReportingTool.RiskManagement.Data.Models;

public record FloodReport
{
    public required Guid Id { get; init; }
    public required Guid EligibilityCheckId { get; init; }
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset? UpdatedUtc { get; init; }
    public required long Uprn { get; init; }
    public required double Easting { get; init; }
    public required double Northing { get; init; }
    public DateTimeOffset? ImpactStartUtc { get; init; }
    public int ImpactDurationHours { get; init; }
}
