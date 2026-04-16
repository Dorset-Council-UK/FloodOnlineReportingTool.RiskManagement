using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchResultTable
{
    [Parameter, EditorRequired]
    public GridItemsProvider<FloodResultItem> ItemsProvider { get; set; } = default!;

    private readonly PaginationState _pagination = new() { ItemsPerPage = 10 };
    private QuickGrid<FloodResultItem> _grid = default!;
}
