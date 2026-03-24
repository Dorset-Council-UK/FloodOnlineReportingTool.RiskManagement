using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Services.SavedStates;

public class SearchResultsState : ISearchResultsState
{
    public SearchResultsViewModel? Options { get; private set; }
    public bool HasOptions => Options != null;

    public void SetResultOptions(SearchResultsViewModel options)
    {
        Options = options;
    }
}
