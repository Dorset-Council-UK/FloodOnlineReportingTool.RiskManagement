using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchResultTable
{
    [Parameter, EditorRequired]
    public PagedResult<FloodReportTableRow> SearchResults { get; set; } = default!;

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
            .OrderBy(r => r.Reference)
            .SelectMany(r => r.SourceRecords.Select(s => new FlatSourceRow(
                r.Reference,
                s.Reference,
                s.ViewUri,
                s.ImpactStartUtc,
                s.CreatedUtc,
                s.Causes.Count > 0 ? string.Join(", ", s.Causes.Select(c => c.Name)) : "No flooding sources identified"
            )))
            .AsQueryable();

    private record FlatSourceRow(
        string ReportReference,
        string SourceReference,
        Uri ViewUri,
        DateTimeOffset? ImpactStartUtc,
        DateTimeOffset CreatedUtc,
        string CausesDisplay
    );
}
