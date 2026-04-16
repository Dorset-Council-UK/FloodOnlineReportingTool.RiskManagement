using FloodOnlineReportingTool.Contracts.Shared.Search;
using NetTopologySuite.Geometries;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public class FloodResultItem : ISearchResultFloodReportSource
{
    public Guid Id { get; init; }
    public string RecordStatus { get; init; }
    public Guid SourceId { get; init; }
    public required string Reference { get; init; }
    public required Uri ViewUri { get; init; }
    public DateTimeOffset CreatedUtc { get; init; }
    public DateTimeOffset? UpdatedUtc { get; init; }
    public required Point Geom { get; init; }
    public double Easting { get; init; }
    public double Northing { get; init; }
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public DateTimeOffset? ImpactStartUtc { get; init; }
    public IReadOnlyCollection<SearchResultFloodReportCause> Causes { get; init; }
    public IReadOnlyCollection<SearchResultFloodReportOrganisation> Organisations { get; init; }
}
