using FloodOnlineReportingTool.Contracts.Shared.Search;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchResultTable
{
    [Parameter, EditorRequired]
    public GridItemsProvider<SearchResultFloodReport> ItemsProvider { get; set; } = default!;

    private readonly PaginationState _pagination = new() { ItemsPerPage = 10 };
    private QuickGrid<SearchResultFloodReport> _grid = default!;
}
