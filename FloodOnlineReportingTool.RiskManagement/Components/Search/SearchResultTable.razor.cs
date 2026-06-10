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

    private void Toggle(Guid id)
    {
        if (!_expandedRows.Remove(id))
        {
            _expandedRows.Add(id);
        }
    }
}
