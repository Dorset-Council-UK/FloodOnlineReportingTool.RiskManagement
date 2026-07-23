using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchResultTable
{
    [Parameter, EditorRequired]
    public SearchFloodReportPagedResult<FloodReportTableRow> SearchResults { get; set; } = default!;

    [Parameter]
    public EventCallback<int> OnPageChanged { get; set; }

    private readonly HashSet<Guid> _expandedRows = [];
    private bool _showGrouped = true;

    private void Toggle(Guid id)
    {
        if (!_expandedRows.Remove(id))
        {
            _expandedRows.Add(id);
        }
    }

    private IQueryable<FlatSourceRow> FlatItems =>
        SearchResults.Results
            .OrderBy(r => r.Reference, StringComparer.OrdinalIgnoreCase)
            .SelectMany(r => r.SourceRecords.Select(s => new FlatSourceRow(
                r.Reference,
                s.Reference,
                r.RecordStatus,
                s.ViewUri,
                s.ImpactStartUtc,
                s.Causes.Count == 1 ? s.Causes.First().Name : $"{s.Causes.Count} causes"
            )))
            .AsQueryable();

    private record FlatSourceRow(
        string ReportReference,
        string SourceReference,
        string RecordStatus,
        Uri ViewUri,
        DateTimeOffset? ImpactStartUtc,
        string CausesDisplay
    );
}
