using FloodOnlineReportingTool.Contracts.Shared.Search;

namespace FloodOnlineReportingTool.RiskManagement.Models.Report;

public record ReportOverviewViewModel
{
    //[GdsFieldErrorClass(GdsFieldTypes.Input)]
    //public string? UprnText { get; set; }
    //public long? Uprn => long.TryParse(UprnText, CultureInfo.InvariantCulture, out var value) ? value : null;

    public Guid Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public Guid RecordStatusId { get; init; }

    public IReadOnlyCollection<SearchResultFloodReportSource> SourceRecords { get; init; } = [];
}
