using FloodOnlineReportingTool.Contracts.Shared.Search;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchResultList() {
    [Parameter]
    public PagedResult<SearchResultFloodReport> SearchResults { get; set; }

    [Parameter]
    public EventCallback<int> OnPageChanged { get; set; }

}
