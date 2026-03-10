using FloodOnlineReportingTool.RiskManagement.Models.Search;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface ISearchOptionsState
{
    SearchOptionsViewModel? Options { get; }
    bool HasOptions { get; }
    void SetOptions(SearchOptionsViewModel options);
}
