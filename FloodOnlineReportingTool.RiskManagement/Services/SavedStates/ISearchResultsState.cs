using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Services.SavedStates;

public interface ISearchResultsState
{
    SearchResultsViewModel? Options { get; }
    bool HasOptions { get; }
    void SetResultOptions(SearchResultsViewModel options);
}
