using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Services.SavedStates;

public class SearchOptionsState : ISearchOptionsState
{
    public SearchOptionsViewModel? Options { get; private set; }
    public bool HasOptions => Options != null;

    public void SetOptions(SearchOptionsViewModel options)
    {
        Options = options;
    }
}
