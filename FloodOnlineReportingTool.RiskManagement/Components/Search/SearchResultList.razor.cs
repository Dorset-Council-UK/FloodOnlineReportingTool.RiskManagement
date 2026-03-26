using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchResultList() {
    [Parameter]
    public PagedResult<SearchResultFloodReport> SearchResults { get; set; }

    [Parameter]
    public EventCallback<int> OnPageChanged { get; set; }

}
