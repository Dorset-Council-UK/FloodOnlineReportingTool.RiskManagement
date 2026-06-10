using FloodOnlineReportingTool.Contracts.Shared.Search;

namespace FloodOnlineReportingTool.RiskManagement.Models.Search;

public class FloodReportTableRow
{
    public Guid Id { get; init; }
    public required string Reference { get; init; }
    public required string RecordStatus { get; init; }
    public IReadOnlyCollection<SearchResultFloodReportSource> SourceRecords { get; init; } = [];
}
